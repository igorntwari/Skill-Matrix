using Microsoft.EntityFrameworkCore;
using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Infrastructure.Data;
using HotChocolate.Authorization;
using HotChocolate.Data;
using SkillMatchPro.Application.Services;
using SkillMatchPro.Domain.Enums;
using SkillMatchPro.Domain.ValueObjects;
using System.Security.Claims;
using SkillMatchPro.Application.Scoring;
using SkillMatchPro.API.GraphQL.Types;

namespace SkillMatchPro.API.GraphQL;


public class Query
{
    public string Hello() => "Hello from SkillMatch Pro!";

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize]
    public async Task<List<Employee>> GetEmployees([Service] ApplicationDbContext context)
    {
        return await context.Employees.ToListAsync();
    }

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Roles = new[] { "Admin" })]
    public async Task<List<Skill>> GetSkills([Service] ApplicationDbContext context)
    {
        return await context.Skills.ToListAsync();
    }

    public async Task<List<Employee>> GetEmployeesWithSkills([Service] ApplicationDbContext context)
    {
        return await context.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .ToListAsync();
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<List<User>> GetAllUsers([Service] ApplicationDbContext context)
    {
        return await context.Users
            .Include(u => u.Employee)
            .ToListAsync();
    }


[UseProjection]
[UseFiltering]
[UseSorting]
[Authorize]
public IQueryable<Employee> GetFilteredEmployees([Service] ApplicationDbContext context)
{
    return context.Employees
        .Include(e => e.EmployeeSkills)
        .ThenInclude(es => es.Skill);
}

[UseProjection]
[UseFiltering]
[UseSorting]
[Authorize]
public IQueryable<Skill> GetFilteredSkills([Service] ApplicationDbContext context)
{
    return context.Skills;
}

[Authorize]
public async Task<List<Employee>> SearchEmployees(
    string searchTerm,
    [Service] ApplicationDbContext context)
{
    var normalizedSearch = searchTerm.ToLower();

    return await context.Employees
        .Include(e => e.EmployeeSkills)
        .ThenInclude(es => es.Skill)
        .Where(e =>
            e.FirstName.ToLower().Contains(normalizedSearch) ||
            e.LastName.ToLower().Contains(normalizedSearch) ||
            e.Email.ToLower().Contains(normalizedSearch) ||
            e.Department.ToLower().Contains(normalizedSearch) ||
            e.Title.ToLower().Contains(normalizedSearch))
        .ToListAsync();
}
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize]
    public IQueryable<Employee> GetPaginatedEmployees([Service] ApplicationDbContext context)
    {
        return context.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill);
    }

    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize]
    public IQueryable<Skill> GetPaginatedSkills([Service] ApplicationDbContext context)
    {
        return context.Skills;
    }

    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Policy = "ManagerOrAbove")]
    public IQueryable<Employee> GetEmployeeDirectory([Service] ApplicationDbContext context)
    {
        return context.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill);
    }

    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<TeamComposition> GetTeamComposition(
    Guid projectId,
    [Service] IMatchingService matchingService,
    [Service] IHttpContextAccessor httpContextAccessor)
    {
        var requestedBy = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

        return await matchingService.GetTeamComposition(projectId, requestedBy);
    }

    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<List<MatchCandidate>> GetCandidatesForSkill(
        Guid skillId,
        ProficiencyLevel minProficiency,
        [Service] IMatchingService matchingService)
    {
        // Default to next 3 months for availability check
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddMonths(3);

        return await matchingService.FindCandidatesForSkill(
            skillId, minProficiency, startDate, endDate, 100);
    }
    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<List<Project>> GetProjects([Service] ApplicationDbContext context)
    {
        return await context.Projects
            .Include(p => p.Requirements)
            .ThenInclude(r => r.Skill)
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }

    // Also add a filtered version
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Policy = "ManagerOrAbove")]
    public IQueryable<Project> GetFilteredProjects([Service] ApplicationDbContext context)
    {
        return context.Projects
            .Include(p => p.Requirements)
            .ThenInclude(r => r.Skill)
            .Include(p => p.Assignments)
            .ThenInclude(a => a.Employee);
    }

    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<AdvancedMatchResult> GetAdvancedTeamComposition(
    Guid projectId,
    [Service] IAdvancedMatchingService advancedMatchingService,
    [Service] IHttpContextAccessor httpContextAccessor)
    {
        var requestedBy = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

        return await advancedMatchingService.GetTeamCompositionWithScoring(
            projectId, requestedBy);
    }

    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<AdvancedMatchResult> GetCustomScoredTeamComposition(
        Guid projectId,
        decimal? proficiencyWeight,
        decimal? availabilityWeight,
        decimal? performanceWeight,
        decimal? chemistryWeight,
        decimal? workloadWeight,
        decimal? experienceWeight,
        [Service] IAdvancedMatchingService advancedMatchingService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var requestedBy = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

        ScoringConfiguration? customWeights = null;
        if (proficiencyWeight.HasValue || availabilityWeight.HasValue ||
            performanceWeight.HasValue || chemistryWeight.HasValue ||
            workloadWeight.HasValue || experienceWeight.HasValue)
        {
            customWeights = new ScoringConfiguration();

            if (proficiencyWeight.HasValue)
                customWeights.ComponentWeights["Proficiency"] = proficiencyWeight.Value;
            if (availabilityWeight.HasValue)
                customWeights.ComponentWeights["Availability"] = availabilityWeight.Value;
            if (performanceWeight.HasValue)
                customWeights.ComponentWeights["Performance"] = performanceWeight.Value;
            if (chemistryWeight.HasValue)
                customWeights.ComponentWeights["TeamChemistry"] = chemistryWeight.Value;
            if (workloadWeight.HasValue)
                customWeights.ComponentWeights["WorkloadBalance"] = workloadWeight.Value;
            if (experienceWeight.HasValue)
                customWeights.ComponentWeights["Experience"] = experienceWeight.Value;
        }

        return await advancedMatchingService.GetTeamCompositionWithScoring(
            projectId, requestedBy, customWeights);
    }
    [Authorize]
    public async Task<Employee?> GetEmployee(
    Guid id,
    [Service] ApplicationDbContext context)
    {
        return await context.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<TeamOptimizationResultType> GetOptimizedTeamOptions(
    Guid projectId,
    OptimizationConstraintsInput? constraints,
    [Service] ITeamOptimizationService optimizationService,
    [Service] ApplicationDbContext context)
    {
        var optimizationConstraints = constraints != null
            ? new OptimizationConstraints
            {
                MaxBudgetPerWeek = constraints.MaxBudgetPerWeek,
                MaxTeamSize = constraints.MaxTeamSize,
                MinSeniorMembers = constraints.MinSeniorMembers,
                RequireBackupCoverage = constraints.RequireBackupCoverage,
                MinBufferHoursPerPerson = constraints.MinBufferHoursPerPerson,
                RequiredEmployees = constraints.RequiredEmployees,
                ExcludedEmployees = constraints.ExcludedEmployees
            }
            : new OptimizationConstraints();

        var result = await optimizationService.OptimizeTeam(projectId, optimizationConstraints);

        // Map to GraphQL types with additional employee details
        var mappedResult = new TeamOptimizationResultType
        {
            Warnings = result.Warnings,
            Recommendations = result.Recommendations,
            Summary = new TeamAllocationSummaryType
            {
                TotalHoursRequired = result.Summary.TotalHoursRequired,
                TotalHoursAllocated = result.Summary.TotalHoursAllocated,
                AverageUtilization = result.Summary.AverageUtilization,
                HoursBySkill = result.Summary.HoursBySkill,
                HoursBySeniority = result.Summary.HoursBySeniority
            }
        };

        // Map each option with employee details
        foreach (var option in result.Options)
        {
            var mappedOption = new TeamOptionType
            {
                Id = option.Id,
                OptionName = option.OptionName,
                TotalCostPerWeek = option.TotalCostPerWeek,
                QualityScore = option.QualityScore,
                RiskScore = option.RiskScore,
                MeetsAllRequirements = option.MeetsAllRequirements,
                TotalHoursPerWeek = option.GetTotalHoursPerWeek(),
                EffectiveHoursPerWeek = option.GetEffectiveHoursPerWeek(),
                TradeOffs = option.TradeOffs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Allocations = new List<HourAllocationDetail>()
            };

            // Get employee details for each allocation
            foreach (var allocation in option.Allocations)
            {
                var employee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Id == allocation.EmployeeId);

                if (employee != null)
                {
                    mappedOption.Allocations.Add(new HourAllocationDetail
                    {
                        EmployeeId = allocation.EmployeeId,
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        SkillName = allocation.AllocationNote, // Extract from note for now
                        HoursPerWeek = allocation.HoursPerWeek,
                        EffortMultiplier = allocation.EffortMultiplier,
                        EffectiveHours = allocation.GetEffectiveHours(),
                        AllocationNote = allocation.AllocationNote,
                        WeeklyCost = allocation.HoursPerWeek * GetHourlyCost(employee)
                    });
                }
            }

            mappedResult.Options.Add(mappedOption);
        }

        return mappedResult;
    }

    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<List<HourlyAvailabilityType>> GetTeamHourlyAvailability(
        List<Guid> employeeIds,
        DateTime startDate,
        DateTime endDate,
        [Service] ITeamOptimizationService optimizationService,
        [Service] ApplicationDbContext context)
    {
        var availability = await optimizationService.GetTeamAvailability(
            employeeIds, startDate, endDate);

        var result = new List<HourlyAvailabilityType>();

        foreach (var avail in availability)
        {
            var employee = await context.Employees
                .FirstOrDefaultAsync(e => e.Id == avail.EmployeeId);

            if (employee != null)
            {
                var mapped = new HourlyAvailabilityType
                {
                    EmployeeId = avail.EmployeeId,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    EmployeeTitle = employee.Title,
                    TotalHoursPerWeek = avail.TotalHoursPerWeek,
                    AllocatedHours = avail.AllocatedHours,
                    AvailableHours = avail.AvailableHours,
                    UtilizationPercentage = avail.TotalHoursPerWeek > 0
                        ? Math.Round((decimal)avail.AllocatedHours / avail.TotalHoursPerWeek * 100, 2)
                        : 0,
                    CurrentProjects = avail.CurrentProjects.Select(p => new ProjectHoursType
                    {
                        ProjectId = Guid.NewGuid(), // Would need to enhance model
                        ProjectName = p.ProjectName,
                        HoursPerWeek = p.HoursPerWeek,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate
                    }).ToList()
                };

                result.Add(mapped);
            }
        }

        return result;
    }

    // Helper method (add to Query class)
    private decimal GetHourlyCost(Employee employee)
    {
        return employee.Title.ToLower() switch
        {
            var t when t.Contains("junior") => 50m,
            var t when t.Contains("senior") => 120m,
            var t when t.Contains("lead") => 150m,
            var t when t.Contains("principal") => 180m,
            _ => 80m
        };
    }
    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<TeamOptimizationResultType> GetSimpleOptimizedTeam(
    Guid projectId,
    [Service] ITeamOptimizationService optimizationService,
    [Service] ApplicationDbContext context)
    {
        // Use default constraints
        var constraints = new OptimizationConstraints();
        var result = await optimizationService.OptimizeTeam(projectId, constraints);

        // Map to GraphQL types (same mapping code as before)
        var mappedResult = new TeamOptimizationResultType
        {
            Warnings = result.Warnings,
            Recommendations = result.Recommendations,
            Summary = new TeamAllocationSummaryType
            {
                TotalHoursRequired = result.Summary.TotalHoursRequired,
                TotalHoursAllocated = result.Summary.TotalHoursAllocated,
                AverageUtilization = result.Summary.AverageUtilization,
                HoursBySkill = result.Summary.HoursBySkill,
                HoursBySeniority = result.Summary.HoursBySeniority
            },
            Options = new List<TeamOptionType>()
        };

        // Map options...
        foreach (var option in result.Options)
        {
            var mappedOption = new TeamOptionType
            {
                Id = option.Id,
                OptionName = option.OptionName,
                TotalCostPerWeek = option.TotalCostPerWeek,
                QualityScore = option.QualityScore,
                RiskScore = option.RiskScore,
                MeetsAllRequirements = option.MeetsAllRequirements,
                TotalHoursPerWeek = option.GetTotalHoursPerWeek(),
                EffectiveHoursPerWeek = option.GetEffectiveHoursPerWeek(),
                TradeOffs = option.TradeOffs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Allocations = new List<HourAllocationDetail>()
            };

            foreach (var allocation in option.Allocations)
            {
                var employee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Id == allocation.EmployeeId);

                if (employee != null)
                {
                    mappedOption.Allocations.Add(new HourAllocationDetail
                    {
                        EmployeeId = allocation.EmployeeId,
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        SkillName = allocation.AllocationNote,
                        HoursPerWeek = allocation.HoursPerWeek,
                        EffortMultiplier = allocation.EffortMultiplier,
                        EffectiveHours = allocation.GetEffectiveHours(),
                        AllocationNote = allocation.AllocationNote,
                        WeeklyCost = allocation.HoursPerWeek * GetHourlyCost(employee)
                    });
                }
            }

            mappedResult.Options.Add(mappedOption);
        }


        return mappedResult;
    }

    [Authorize]
    public async Task<ProjectTeamSummary> GetProjectTeam(
    Guid projectId,
    [Service] ApplicationDbContext context)
    {
        var project = await context.Projects
            .Include(p => p.Requirements)
            .ThenInclude(r => r.Skill)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            throw new GraphQLException("Project not found");

        var assignments = await context.ProjectAssignments
            .Include(pa => pa.Employee)
            .ThenInclude(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .Where(pa => pa.ProjectId == projectId && pa.IsActive)
            .ToListAsync();

        var summary = new ProjectTeamSummary
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            TotalHoursAllocated = assignments.Sum(a => (int)(a.AllocationPercentage / 100.0 * 40)),
            TeamMembers = assignments.Select(a => new TeamMemberSummary
            {
                AssignmentId = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = $"{a.Employee.FirstName} {a.Employee.LastName}",
                Role = a.Role,
                HoursPerWeek = (int)(a.AllocationPercentage / 100.0 * 40),
                Skills = a.Employee.EmployeeSkills.Select(es => es.Skill.Name).ToList(),
                StartDate = a.StartDate,
                EndDate = a.EndDate
            }).ToList()
        };

        // Check coverage
        foreach (var req in project.Requirements)
        {
            var coverage = assignments
                .Where(a => a.Employee.EmployeeSkills.Any(es => es.SkillId == req.SkillId))
                .Sum(a => (int)(a.AllocationPercentage / 100.0 * 40));

            summary.SkillCoverage[req.Skill.Name] = new SkillCoverageInfo
            {
                RequiredHours = req.RequiredCount * 36, // Assuming 36 hours per person
                AllocatedHours = coverage,
                IsCovered = coverage >= req.RequiredCount * 36
            };
        }

        return summary;
    }
}


