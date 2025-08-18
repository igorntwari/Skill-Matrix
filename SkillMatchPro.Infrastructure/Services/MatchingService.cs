using Microsoft.EntityFrameworkCore;
using SkillMatchPro.Application.Services;
using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Domain.Enums;
using SkillMatchPro.Domain.ValueObjects;
using SkillMatchPro.Infrastructure.Data;

namespace SkillMatchPro.Infrastructure.Services;

public class MatchingService : IMatchingService
{
    private readonly ApplicationDbContext _context;
    private readonly IAllocationService _allocationService;

    public MatchingService(ApplicationDbContext context, IAllocationService allocationService)
    {
        _context = context;
        _allocationService = allocationService;
    }

    public async Task<TeamComposition> GetTeamComposition(Guid projectId, string requestedBy)
    {
        // Get project with requirements
        var project = await _context.Projects
            .Include(p => p.Requirements)
            .ThenInclude(r => r.Skill)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            throw new ArgumentException("Project not found");

        var teamComposition = new TeamComposition(projectId, requestedBy);

        // Process each skill requirement
        foreach (var requirement in project.Requirements)
        {
            var skillMatch = new SkillMatch(
                requirement.Skill,
                requirement.MinimumProficiency,
                requirement.RequiredCount);

            // Find candidates for this skill
            var candidates = await FindCandidatesForSkill(
                requirement.SkillId,
                requirement.MinimumProficiency,
                project.StartDate,
                project.EndDate,
                100); // For now, assume 100% allocation per skill

            foreach (var candidate in candidates)
            {
                skillMatch.AddCandidate(candidate);
            }

            teamComposition.AddSkillMatch(skillMatch);

            // Add top candidates to team
            var topCandidates = skillMatch.GetTopCandidates();
            foreach (var candidate in topCandidates)
            {
                teamComposition.AddTeamMember(
                    candidate.Employee,
                    requirement.Skill,
                    100, // Allocation percentage
                    candidate.Score);
            }
        }

        if (teamComposition.AreAllRequirementsMet())
        {
            teamComposition.MarkAsOptimal();
        }

        return teamComposition;
    }

    public async Task<List<MatchCandidate>> FindCandidatesForSkill(
        Guid skillId,
        ProficiencyLevel requiredProficiency,
        DateTime startDate,
        DateTime endDate,
        int requiredAllocation)
    {
        // Get all employees with the required skill
        var employeesWithSkill = await _context.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .Include(e => e.ProjectAssignments)
            .Where(e => e.EmployeeSkills.Any(es =>
                es.SkillId == skillId &&
                es.Proficiency >= requiredProficiency))
            .ToListAsync();

        var candidates = new List<MatchCandidate>();

        foreach (var employee in employeesWithSkill)
        {
            // Check availability
            var hasConflict = await _allocationService.CheckAllocationConflict(
                employee.Id, requiredAllocation, startDate, endDate);

            var currentAllocation = employee.GetCurrentAllocationPercentage();
            var isAvailable = !hasConflict;

            // Get the specific skill
            var skill = employee.EmployeeSkills
                .First(es => es.SkillId == skillId)
                .Skill;

            // Calculate match score
            var score = await CalculateMatchScore(employee, skill!, requiredProficiency);

            var candidate = new MatchCandidate(
                employee,
                skill!,
                score,
                isAvailable,
                currentAllocation);

            candidates.Add(candidate);
        }

        return candidates.OrderByDescending(c => c.Score.TotalScore).ToList();
    }

    public Task<MatchScore> CalculateMatchScore(
    Employee employee,
    Skill requiredSkill,
    ProficiencyLevel requiredProficiency)
    {
        var employeeSkill = employee.EmployeeSkills
            .FirstOrDefault(es => es.SkillId == requiredSkill.Id);

        if (employeeSkill == null)
            return Task.FromResult(new MatchScore(0, 0, 0, "Employee doesn't have the required skill"));

        // Calculate proficiency score (0-100)
        var proficiencyScore = CalculateProficiencyScore(
            employeeSkill.Proficiency,
            requiredProficiency);

        // Calculate availability score (0-100)
        var currentAllocation = employee.GetCurrentAllocationPercentage();
        var availabilityScore = 100 - currentAllocation;

        // Calculate experience score based on how long they've had the skill
        // AcquiredDate is NOT nullable
        var monthsWithSkill = (int)((DateTime.UtcNow - employeeSkill.AcquiredDate).TotalDays / 30);
        var experienceScore = Math.Min(100, monthsWithSkill * 2); // 2 points per month, max 100

        var explanation = GenerateExplanation(
            employeeSkill.Proficiency,
            requiredProficiency,
            currentAllocation,
            monthsWithSkill);

        return Task.FromResult(new MatchScore(
            proficiencyScore,
            availabilityScore,
            experienceScore,
            explanation));
    }

    private decimal CalculateProficiencyScore(
        ProficiencyLevel actual,
        ProficiencyLevel required)
    {
        var difference = (int)actual - (int)required;

        return difference switch
        {
            >= 2 => 100,    // Much higher than required
            1 => 90,        // Higher than required
            0 => 80,        // Exact match
            -1 => 60,       // One level below
            _ => 40         // Much lower
        };
    }

    private string GenerateExplanation(
        ProficiencyLevel actual,
        ProficiencyLevel required,
        int currentAllocation,
        int monthsExperience)
    {
        var parts = new List<string>();

        if (actual >= required)
            parts.Add($"Meets proficiency requirement ({actual} vs {required} required)");
        else
            parts.Add($"Below required proficiency ({actual} vs {required} required)");

        parts.Add($"Currently {currentAllocation}% allocated");
        parts.Add($"{monthsExperience} months of experience with this skill");

        return string.Join(". ", parts);
    }
}