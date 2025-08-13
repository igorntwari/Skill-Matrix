using SkillMatchPro.Application.Scoring;
using SkillMatchPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SkillMatchPro.Domain.Entities;

namespace SkillMatchPro.Infrastructure.Scoring;

public class TeamChemistryScorer : IScoringComponent
{
    private readonly ApplicationDbContext _context;

    public string Name => "TeamChemistry";
    public decimal DefaultWeight => 0.15m;

    public TeamChemistryScorer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ComponentScore> CalculateScore(ScoringContext context)
    {
        if (!context.CurrentTeamMembers.Any())
        {
            return new ComponentScore
            {
                Score = 75,
                Explanation = "No existing team members to evaluate chemistry",
                Confidence = 0.5m
            };
        }

        var candidateId = context.Candidate.Id;
        var teamMemberIds = context.CurrentTeamMembers.Select(m => m.Id).ToList();

        // Get all past collaborations
        var collaborations = await _context.Set<TeamCollaboration>()
            .Where(tc =>
                (tc.Employee1Id == candidateId && teamMemberIds.Contains(tc.Employee2Id)) ||
                (tc.Employee2Id == candidateId && teamMemberIds.Contains(tc.Employee1Id)))
            .ToListAsync();

        if (!collaborations.Any())
        {
            return new ComponentScore
            {
                Score = 70,
                Explanation = "No previous collaboration history with team members",
                Confidence = 0.4m
            };
        }

        // Group by team member
        var chemistryByMember = new Dictionary<Guid, List<decimal>>();
        foreach (var collab in collaborations)
        {
            var teammateId = collab.Employee1Id == candidateId ? collab.Employee2Id : collab.Employee1Id;
            if (!chemistryByMember.ContainsKey(teammateId))
                chemistryByMember[teammateId] = new List<decimal>();
            chemistryByMember[teammateId].Add(collab.CollaborationScore);
        }

        // Calculate average chemistry per teammate
        var avgChemistryScores = chemistryByMember
            .Select(kvp => kvp.Value.Average())
            .ToList();

        var overallChemistry = avgChemistryScores.Average();
        var wouldWorkAgainRate = collaborations.Count(c => c.WouldWorkTogetherAgain) / (decimal)collaborations.Count * 100;

        // Weighted score
        var score = (overallChemistry * 0.7m) + (wouldWorkAgainRate * 0.3m);

        var details = new Dictionary<string, object>
        {
            ["AverageChemistry"] = Math.Round(overallChemistry, 1),
            ["WouldWorkAgainRate"] = Math.Round(wouldWorkAgainRate, 1),
            ["CollaborationsCount"] = collaborations.Count,
            ["UniqueTeammates"] = chemistryByMember.Count
        };

        return new ComponentScore
        {
            Score = Math.Round(score, 2),
            Explanation = $"Worked with {chemistryByMember.Count} current team members before, {wouldWorkAgainRate:F0}% would work together again",
            Confidence = Math.Min(1m, collaborations.Count / 10m), // Full confidence at 10+ collaborations
            Details = details
        };
    }
}