using Microsoft.EntityFrameworkCore;
using SkillMatchPro.Application.Services;
using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Domain.ValueObjects;
using SkillMatchPro.Infrastructure.Data;

namespace SkillMatchPro.Infrastructure.Services;

public class TeamOptimizationService : ITeamOptimizationService
{
    private readonly ApplicationDbContext _context;
    private readonly IAdvancedMatchingService _matchingService;

    public TeamOptimizationService(
        ApplicationDbContext context,
        IAdvancedMatchingService matchingService)
    {
        _context = context;
        _matchingService = matchingService;
    }

    private async Task<List<ScoredCandidate>> GetCandidatesForRequirement(
    ProjectRequirement requirement,
    Project project)
    {
        return await _matchingService.GetScoredCandidatesForSkill(
            requirement.SkillId,
            requirement.MinimumProficiency,
            project,
            new List<Employee>(), // Empty for now, will be updated
            null);
    }

    private decimal CalculateQualityScore(TeamOption option)
    {
        if (!option.Allocations.Any()) return 0;

        var score = 0m;
        var totalWeight = 0m;

        foreach (var allocation in option.Allocations)
        {
            // Get employee details
            var employee = _context.Employees
                .FirstOrDefault(e => e.Id == allocation.EmployeeId);

            if (employee == null) continue;

            var weight = allocation.HoursPerWeek;
            totalWeight += weight;

            // Score based on seniority (simplified)
            var seniorityScore = employee.Title.ToLower() switch
            {
                var t when t.Contains("principal") => 100m,
                var t when t.Contains("lead") => 90m,
                var t when t.Contains("senior") => 80m,
                var t when t.Contains("junior") => 60m,
                _ => 70m
            };

            // Factor in effort multiplier (better developers = higher quality)
            var qualityMultiplier = 2m - allocation.EffortMultiplier; // Inverts the multiplier
            score += seniorityScore * qualityMultiplier * weight;
        }

        return totalWeight > 0 ? Math.Round(score / totalWeight, 2) : 0;
    }

    private decimal CalculateRiskScore(TeamOption option)
    {
        var risks = new List<decimal>();

        // Risk 1: Bus factor (single point of failure)
        var skillCoverage = option.Allocations
            .GroupBy(a => a.ProjectId) // Group by skill (simplified)
            .Select(g => g.Count())
            .ToList();

        var singlePointsOfFailure = skillCoverage.Count(c => c == 1);
        var busFactorRisk = singlePointsOfFailure * 20m; // 20 points per SPOF
        risks.Add(Math.Min(100, busFactorRisk));

        // Risk 2: Team size (too small = high risk)
        var uniqueEmployees = option.Allocations.Select(a => a.EmployeeId).Distinct().Count();
        var teamSizeRisk = uniqueEmployees switch
        {
            1 => 90m,   // Very high risk
            2 => 70m,   // High risk
            3 => 40m,   // Medium risk
            4 => 20m,   // Low risk
            _ => 10m    // Very low risk
        };
        risks.Add(teamSizeRisk);

        // Risk 3: Junior heavy team
        var juniorHours = option.Allocations
            .Where(a => a.EffortMultiplier > 1.2m) // Junior developers
            .Sum(a => a.HoursPerWeek);
        var totalHours = option.GetTotalHoursPerWeek();
        var juniorRatio = totalHours > 0 ? juniorHours / (decimal)totalHours : 0;
        var juniorRisk = juniorRatio * 100m;
        risks.Add(juniorRisk);

        // Average all risks
        return Math.Round(risks.Average(), 2);
    }


    private bool CheckRequirementsMet(TeamOption option, Project project)
    {
        foreach (var requirement in project.Requirements)
        {
            var skillHours = option.Allocations
                .Where(a => true)
                .Sum(a => a.GetEffectiveHours());

            var requiredHours = requirement.RequiredCount * 36m; // 36 hours per person

            if (skillHours < requiredHours * 0.9m) // Allow 10% tolerance
                return false;
        }

        return true;
    }

    private TeamAllocationSummary GenerateSummary(Project project, List<TeamOption> options)
    {
        var summary = new TeamAllocationSummary();

        // Calculate total required hours
        summary.TotalHoursRequired = project.Requirements.Sum(r => r.RequiredCount * 36);

        // Get average allocated hours across options
        if (options.Any())
        {
            summary.TotalHoursAllocated = (int)options.Average(o => o.GetTotalHoursPerWeek());

            var uniqueEmployees = options
                .SelectMany(o => o.Allocations.Select(a => a.EmployeeId))
                .Distinct()
                .Count();

            summary.AverageUtilization = uniqueEmployees > 0
                ? Math.Round(summary.TotalHoursAllocated / (decimal)(uniqueEmployees * 36) * 100, 2)
                : 0;
        }

        // Hours by skill (simplified - would need skill tracking in allocations)
        foreach (var req in project.Requirements)
        {
            summary.HoursBySkill[req.Skill.Name] = req.RequiredCount * 36;
        }

        return summary;
    }

    private List<string> GenerateWarnings(List<TeamOption> options, OptimizationConstraints constraints)
    {
        var warnings = new List<string>();

        foreach (var option in options)
        {
            // Budget warning
            if (constraints.MaxBudgetPerWeek.HasValue &&
                option.TotalCostPerWeek > constraints.MaxBudgetPerWeek.Value)
            {
                warnings.Add($"{option.OptionName} exceeds budget by ${option.TotalCostPerWeek - constraints.MaxBudgetPerWeek.Value:F2}/week");
            }

            // Team size warning
            var teamSize = option.Allocations.Select(a => a.EmployeeId).Distinct().Count();
            if (constraints.MaxTeamSize.HasValue && teamSize > constraints.MaxTeamSize.Value)
            {
                warnings.Add($"{option.OptionName} has {teamSize} members (max: {constraints.MaxTeamSize})");
            }

            // High risk warning
            if (option.RiskScore > 70)
            {
                warnings.Add($"{option.OptionName} has high risk score: {option.RiskScore}");
            }
        }

        return warnings.Distinct().ToList();
    }

    private Dictionary<string, string> GenerateRecommendations(
    List<TeamOption> options,
    Project project)
    {
        var recommendations = new Dictionary<string, string>();

        // Check if all options are expensive
        var avgCost = options.Average(o => o.TotalCostPerWeek);
        if (avgCost > 5000) // Arbitrary threshold
        {
            recommendations["Cost"] = "Consider extending timeline to reduce weekly team size and cost";
        }

        // Check if all options have high risk
        var avgRisk = options.Average(o => o.RiskScore);
        if (avgRisk > 60)
        {
            recommendations["Risk"] = "All options have elevated risk. Consider adding senior developers or extending timeline";
        }

        // Check quality vs cost trade-off
        var qualityOption = options.OrderByDescending(o => o.QualityScore).First();
        var costOption = options.OrderBy(o => o.TotalCostPerWeek).First();

        if (qualityOption.TotalCostPerWeek > costOption.TotalCostPerWeek * 1.5m)
        {
            recommendations["Trade-off"] = "Significant cost difference between quality and budget options. Consider the balanced approach";
        }

        return recommendations;
    }



    public async Task<TeamOptimizationResult> OptimizeTeam(
        Guid projectId,
        OptimizationConstraints constraints)
    {
        // Get project with all requirements
        var project = await _context.Projects
            .Include(p => p.Requirements)
            .ThenInclude(r => r.Skill)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            throw new ArgumentException("Project not found");

        var result = new TeamOptimizationResult();

        // Generate different team options
        var options = new List<TeamOption>();

        // Option 1: Minimum Cost
        var minCostOption = await GenerateMinimumCostTeam(project, constraints);
        options.Add(minCostOption);

        // Option 2: Maximum Quality
        var maxQualityOption = await GenerateMaximumQualityTeam(project, constraints);
        options.Add(maxQualityOption);

        // Option 3: Balanced
        var balancedOption = await GenerateBalancedTeam(project, constraints);
        options.Add(balancedOption);

        // Option 4: Risk-Averse (more backup coverage)
        var riskAverseOption = await GenerateRiskAverseTeam(project, constraints);
        options.Add(riskAverseOption);

        result.Options = options.Where(o => o != null).ToList();
        result.Summary = GenerateSummary(project, result.Options);
        result.Warnings = GenerateWarnings(result.Options, constraints);
        result.Recommendations = GenerateRecommendations(result.Options, project);

        return result;
    }

    private async Task<TeamOption> GenerateMinimumCostTeam(
        Project project,
        OptimizationConstraints constraints)
    {
        var option = new TeamOption(project.Id, "Minimum Cost");

        // Get candidates sorted by cost (junior developers first)
        foreach (var requirement in project.Requirements)
        {
            var candidates = await GetCandidatesForRequirement(requirement, project);

            // Sort by hourly cost (ascending)
            var sortedByCost = candidates
                .OrderBy(c => GetHourlyCost(c.Employee))
                .ThenByDescending(c => c.TotalScore)
                .ToList();

            // Allocate minimum hours needed
            var hoursNeeded = requirement.RequiredCount * 36; // Assume 36 hours per person
            var hoursAllocated = 0;

            foreach (var candidate in sortedByCost)
            {
                if (hoursAllocated >= hoursNeeded) break;

                var availableHours = await GetAvailableHours(candidate.Employee.Id, project);
                if (availableHours <= constraints.MinBufferHoursPerPerson) continue;

                var hoursToAllocate = Math.Min(
                    availableHours - constraints.MinBufferHoursPerPerson,
                    hoursNeeded - hoursAllocated);

                if (hoursToAllocate >= 8) // Minimum 8 hours to make it worthwhile
                {
                    var allocation = new HourAllocation(
                        project.Id,
                        candidate.Employee.Id,
                        hoursToAllocate,
                        GetEffortMultiplier(candidate.Employee),
                        $"{requirement.Skill.Name} - Cost optimized");

                    option.AddAllocation(allocation, GetHourlyCost(candidate.Employee));
                    hoursAllocated += hoursToAllocate;
                }
            }
        }

        // Calculate scores
        var qualityScore = CalculateQualityScore(option);
        var riskScore = CalculateRiskScore(option);
        var meetsRequirements = CheckRequirementsMet(option, project);

        option.SetScores(qualityScore, riskScore, meetsRequirements);
        option.AddTradeOff("Cost", "Lowest cost option with more junior developers");
        option.AddTradeOff("Quality", "May require more time for tasks due to less experience");
        option.AddTradeOff("Risk", "Higher risk of delays or rework");

        return option;
    }

    private async Task<TeamOption> GenerateMaximumQualityTeam(
    Project project,
    OptimizationConstraints constraints)
    {
        var option = new TeamOption(project.Id, "Maximum Quality");

        foreach (var requirement in project.Requirements)
        {
            var candidates = await GetCandidatesForRequirement(requirement, project);

            // Sort by quality indicators (seniority, performance score)
            var sortedByQuality = candidates
                .OrderByDescending(c => c.TotalScore)
                .ThenBy(c => GetEffortMultiplier(c.Employee))
                .ToList();

            var hoursNeeded = requirement.RequiredCount * 36;
            var hoursAllocated = 0;

            foreach (var candidate in sortedByQuality)
            {
                if (hoursAllocated >= hoursNeeded) break;

                var availableHours = await GetAvailableHours(candidate.Employee.Id, project);
                if (availableHours <= constraints.MinBufferHoursPerPerson) continue;

                var hoursToAllocate = Math.Min(
                    availableHours - constraints.MinBufferHoursPerPerson,
                    hoursNeeded - hoursAllocated);

                if (hoursToAllocate >= 8)
                {
                    var allocation = new HourAllocation(
                        project.Id,
                        candidate.Employee.Id,
                        hoursToAllocate,
                        GetEffortMultiplier(candidate.Employee),
                        $"{requirement.Skill.Name} - Quality optimized");

                    option.AddAllocation(allocation, GetHourlyCost(candidate.Employee));
                    hoursAllocated += hoursToAllocate;
                }
            }
        }

        var qualityScore = CalculateQualityScore(option);
        var riskScore = CalculateRiskScore(option);
        var meetsRequirements = CheckRequirementsMet(option, project);

        option.SetScores(qualityScore, riskScore, meetsRequirements);
        option.AddTradeOff("Quality", "Highest quality with experienced developers");
        option.AddTradeOff("Cost", "Higher cost due to senior resources");
        option.AddTradeOff("Speed", "Faster delivery due to experience");

        return option;
    }

    private async Task<TeamOption> GenerateBalancedTeam(
    Project project,
    OptimizationConstraints constraints)
    {
        var option = new TeamOption(project.Id, "Balanced");

        foreach (var requirement in project.Requirements)
        {
            var candidates = await GetCandidatesForRequirement(requirement, project);

            // Balance between cost and quality
            var sortedBalanced = candidates
                .OrderByDescending(c => c.TotalScore / (1 + GetHourlyCost(c.Employee) / 100))
                .ToList();

            var hoursNeeded = requirement.RequiredCount * 36;
            var hoursAllocated = 0;
            var seniorHours = 0;

            foreach (var candidate in sortedBalanced)
            {
                if (hoursAllocated >= hoursNeeded) break;

                // Ensure mix of seniority levels
                var isSenior = candidate.Employee.Title.ToLower().Contains("senior") ||
                              candidate.Employee.Title.ToLower().Contains("lead");

                if (isSenior && seniorHours > hoursNeeded * 0.4m) continue; // Limit senior allocation

                var availableHours = await GetAvailableHours(candidate.Employee.Id, project);
                if (availableHours <= constraints.MinBufferHoursPerPerson) continue;

                var hoursToAllocate = Math.Min(
                    availableHours - constraints.MinBufferHoursPerPerson,
                    hoursNeeded - hoursAllocated);

                if (hoursToAllocate >= 8)
                {
                    var allocation = new HourAllocation(
                        project.Id,
                        candidate.Employee.Id,
                        hoursToAllocate,
                        GetEffortMultiplier(candidate.Employee),
                        $"{requirement.Skill.Name} - Balanced allocation");

                    option.AddAllocation(allocation, GetHourlyCost(candidate.Employee));
                    hoursAllocated += hoursToAllocate;

                    if (isSenior) seniorHours += hoursToAllocate;
                }
            }
        }

        var qualityScore = CalculateQualityScore(option);
        var riskScore = CalculateRiskScore(option);
        var meetsRequirements = CheckRequirementsMet(option, project);

        option.SetScores(qualityScore, riskScore, meetsRequirements);
        option.AddTradeOff("Balance", "Good mix of experience levels");
        option.AddTradeOff("Cost", "Moderate cost with quality considerations");
        option.AddTradeOff("Mentoring", "Senior members can guide juniors");

        return option;
    }

    private async Task<TeamOption> GenerateRiskAverseTeam(
        Project project,
        OptimizationConstraints constraints)
    {
        var option = new TeamOption(project.Id, "Risk-Averse");

        // Ensure backup coverage for each skill
        // Implementation here...

        return option;
    }

    private async Task<int> GetAvailableHours(Guid employeeId, Project project)
    {
        // Simplified version without WorkSchedule for now
        var defaultHoursPerWeek = 40;
        var bufferHours = 4;

        // Get current allocations
        var currentAllocations = await _context.ProjectAssignments
            .Where(pa => pa.EmployeeId == employeeId &&
                        pa.IsActive &&
                        pa.StartDate <= project.EndDate &&
                        pa.EndDate >= project.StartDate)
            .ToListAsync();

        // Convert percentage allocations to hours
        var allocatedHours = currentAllocations.Sum(pa =>
            (int)(pa.AllocationPercentage / 100.0 * defaultHoursPerWeek));

        // Available hours = total hours - buffer - allocated
        var availableHours = defaultHoursPerWeek - bufferHours - allocatedHours;

        return Math.Max(0, availableHours);
    }

    private decimal GetHourlyCost(Employee employee)
    {
        // Simplified cost model based on title
        return employee.Title.ToLower() switch
        {
            var t when t.Contains("junior") => 50m,
            var t when t.Contains("senior") => 120m,
            var t when t.Contains("lead") => 150m,
            var t when t.Contains("principal") => 180m,
            _ => 80m // Default intermediate rate
        };
    }

    private decimal GetEffortMultiplier(Employee employee)
    {
        // Based on experience level
        return employee.Title.ToLower() switch
        {
            var t when t.Contains("junior") => 1.5m,    // Takes 50% more time
            var t when t.Contains("senior") => 0.8m,    // 20% faster
            var t when t.Contains("lead") => 0.7m,      // 30% faster
            var t when t.Contains("principal") => 0.6m, // 40% faster
            _ => 1.0m // Intermediate baseline
        };
    }

    // Additional helper methods...

    public async Task<List<HourlyAvailability>> GetTeamAvailability(
        List<Guid> employeeIds,
        DateTime startDate,
        DateTime endDate)
    {
        // Implementation to get current availability
        var availability = new List<HourlyAvailability>();

        // Query and calculate availability for each employee
        // ...

        return availability;
    }
}