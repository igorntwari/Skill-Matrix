using Microsoft.EntityFrameworkCore;
using SkillMatchPro.Application.Services;
using SkillMatchPro.Application.Scoring;
using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Domain.Enums;
using SkillMatchPro.Domain.ValueObjects;
using SkillMatchPro.Infrastructure.Data;
using SkillMatchPro.Infrastructure.Scoring;

namespace SkillMatchPro.Infrastructure.Services;

public class AdvancedMatchingService : IAdvancedMatchingService
{
    private readonly ApplicationDbContext _context;
    private readonly IAllocationService _allocationService;
    private readonly List<IScoringComponent> _scoringComponents;

    public AdvancedMatchingService(
        ApplicationDbContext context,
        IAllocationService allocationService)
    {
        _context = context;
        _allocationService = allocationService;

        // Initialize scoring components
        _scoringComponents = new List<IScoringComponent>
        {
            new ProficiencyScorer(),
            new AvailabilityScorer(allocationService),
            new PerformanceScorer(context),
            new TeamChemistryScorer(context),
            new WorkloadBalanceScorer(),
            new ExperienceScorer()
        };
    }

    public async Task<AdvancedMatchResult> GetTeamCompositionWithScoring(
        Guid projectId,
        string requestedBy,
        ScoringConfiguration? customWeights = null)
    {
        var weights = customWeights ?? new ScoringConfiguration();
        weights.ValidateWeights();

        // Get project with requirements
        var project = await _context.Projects
            .Include(p => p.Requirements)
            .ThenInclude(r => r.Skill)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            throw new ArgumentException("Project not found");

        var teamComposition = new TeamComposition(projectId, requestedBy);
        var memberScores = new List<TeamMemberScore>();
        var currentTeam = new List<Employee>();

        // Process each skill requirement
        foreach (var requirement in project.Requirements)
        {
            var candidates = await GetScoredCandidatesForSkill(
                requirement.SkillId,
                requirement.MinimumProficiency,
                project,
                currentTeam,
                weights);

            // Take top candidates
            var topCandidates = candidates
                .Where(c => c.IsAvailable)
                .OrderByDescending(c => c.TotalScore)
                .Take(requirement.RequiredCount)
                .ToList();

            foreach (var candidate in topCandidates)
            {
                // Create old-style MatchScore for compatibility
                var matchScore = new MatchScore(
                    candidate.ComponentScores["Proficiency"].Score,
                    candidate.ComponentScores["Availability"].Score,
                    candidate.ComponentScores.ContainsKey("Experience")
                        ? candidate.ComponentScores["Experience"].Score
                        : 50,
                    $"Advanced scoring: {candidate.TotalScore:F1}/100");

                teamComposition.AddTeamMember(
                    candidate.Employee,
                    requirement.Skill,
                    100, // Allocation percentage
                    matchScore);

                currentTeam.Add(candidate.Employee);
                memberScores.Add(new TeamMemberScore
                {
                    Employee = candidate.Employee,
                    TotalScore = candidate.TotalScore,
                    ComponentScores = candidate.ComponentScores
                });
            }
        }

        // Generate recommendations and risk analysis
        var recommendations = GenerateRecommendations(memberScores, project);
        var risks = AnalyzeRisks(teamComposition, memberScores);

        if (teamComposition.AreAllRequirementsMet())
        {
            teamComposition.MarkAsOptimal();
        }

        return new AdvancedMatchResult
        {
            TeamComposition = teamComposition,
            MemberScores = memberScores,
            Recommendations = recommendations,
            Risks = risks
        };
    }

    public async Task<List<ScoredCandidate>> GetScoredCandidatesForSkill(
        Guid skillId,
        ProficiencyLevel requiredProficiency,
        Project project,
        List<Employee> currentTeam,
        ScoringConfiguration? customWeights = null)
    {
        var weights = customWeights ?? new ScoringConfiguration();

        // Get candidates with the skill
        var candidates = await _context.Employees
            .Include(e => e.EmployeeSkills)
            .ThenInclude(es => es.Skill)
            .Where(e => e.EmployeeSkills.Any(es =>
                es.SkillId == skillId &&
                es.Proficiency >= requiredProficiency))
            .ToListAsync();

        var scoredCandidates = new List<ScoredCandidate>();

        foreach (var candidate in candidates)
        {
            var skill = candidate.EmployeeSkills
                .First(es => es.SkillId == skillId)
                .Skill;

            // Check availability
            var hasConflict = await _allocationService.CheckAllocationConflict(
                candidate.Id, 100, project.StartDate, project.EndDate);

            // Create scoring context
            var context = new ScoringContext
            {
                Candidate = candidate,
                RequiredSkill = skill!,
                RequiredProficiency = requiredProficiency,
                Project = project,
                CurrentTeamMembers = currentTeam,
                ProjectStartDate = project.StartDate,
                ProjectEndDate = project.EndDate
            };

            // Calculate all component scores
            var componentScores = new Dictionary<string, ComponentScore>();
            foreach (var scorer in _scoringComponents)
            {
                componentScores[scorer.Name] = await scorer.CalculateScore(context);
            }

            // Calculate weighted total
            var totalScore = 0m;
            var totalConfidence = 0m;

            foreach (var component in componentScores)
            {
                var weight = weights.ComponentWeights.ContainsKey(component.Key)
                    ? weights.ComponentWeights[component.Key]
                    : 0.1m; // Default weight if not specified

                totalScore += component.Value.Score * weight * component.Value.Confidence;
                totalConfidence += weight * component.Value.Confidence;
            }

            // Normalize by confidence
            if (totalConfidence > 0)
                totalScore = totalScore / totalConfidence;

            scoredCandidates.Add(new ScoredCandidate
            {
                Employee = candidate,
                TotalScore = Math.Round(totalScore, 2),
                ComponentScores = componentScores,
                IsAvailable = !hasConflict,
                CurrentAllocation = candidate.GetCurrentAllocationPercentage()
            });
        }

        return scoredCandidates.OrderByDescending(c => c.TotalScore).ToList();
    }

    private Dictionary<string, string> GenerateRecommendations(
        List<TeamMemberScore> memberScores,
        Project project)
    {
        var recommendations = new Dictionary<string, string>();

        // Check for overloaded members
        var overloaded = memberScores
            .Where(m => m.ComponentScores.ContainsKey("WorkloadBalance") &&
                       m.ComponentScores["WorkloadBalance"].Score < 70)
            .ToList();

        if (overloaded.Any())
        {
            recommendations["Workload"] = $"Consider reducing load on: {string.Join(", ", overloaded.Select(m => m.Employee.FirstName))}";
        }

        // Check for low chemistry scores
        var lowChemistry = memberScores
            .Where(m => m.ComponentScores.ContainsKey("TeamChemistry") &&
                       m.ComponentScores["TeamChemistry"].Score < 60)
            .ToList();

        if (lowChemistry.Any())
        {
            recommendations["TeamBuilding"] = "Schedule team building activities - some members have limited collaboration history";
        }

        // Performance concerns
        var performanceConcerns = memberScores
            .Where(m => m.ComponentScores.ContainsKey("Performance") &&
                       m.ComponentScores["Performance"].Score < 70)
            .ToList();

        if (performanceConcerns.Any())
        {
            recommendations["Monitoring"] = "Close monitoring recommended for team members with recent performance challenges";
        }

        return recommendations;
    }

    private List<string> AnalyzeRisks(TeamComposition team, List<TeamMemberScore> scores)
    {
        var risks = new List<string>();

        // Bus factor risk
        var skillCoverage = team.TeamMembers
            .SelectMany(tm => tm.SkillAssignments)
            .GroupBy(sa => sa.SkillId)
            .Where(g => g.Count() == 1)
            .ToList();

        if (skillCoverage.Any())
        {
            risks.Add($"Single point of failure: {skillCoverage.Count} skills have only one person assigned");
        }

        // New team risk
        var newTeamMembers = scores
            .Where(s => s.ComponentScores.Values.Any(cs => cs.Confidence < 0.5m))
            .ToList();

        if (newTeamMembers.Count > team.TeamMembers.Count / 2)
        {
            risks.Add("Over 50% of team members are new - consider adding experienced members");
        }

        // Overallocation risk
        if (scores.Any(s => s.ComponentScores.ContainsKey("WorkloadBalance") &&
                           s.ComponentScores["WorkloadBalance"].Details["CurrentAllocation"].ToString() == "100"))
        {
            risks.Add("Some team members at 100% allocation - no buffer for emergencies");
        }

        return risks;
    }
}