using SkillMatchPro.Application.Scoring;
using SkillMatchPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SkillMatchPro.Domain.Entities;

namespace SkillMatchPro.Infrastructure.Scoring;

public class PerformanceScorer : IScoringComponent
{
    private readonly ApplicationDbContext _context;

    public string Name => "Performance";
    public decimal DefaultWeight => 0.20m;

    public PerformanceScorer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ComponentScore> CalculateScore(ScoringContext context)
    {
        // Get last 3 project performances
        var performances = await _context.Set<EmployeeProjectPerformance>()
            .Where(p => p.EmployeeId == context.Candidate.Id)
            .OrderByDescending(p => p.EvaluatedAt)
            .Take(3)
            .ToListAsync();

        if (!performances.Any())
        {
            return new ComponentScore
            {
                Score = 70, // Default score for new employees
                Explanation = "New employee - no performance history",
                Confidence = 0.3m
            };
        }

        // Calculate average metrics
        var avgDeliveryRate = performances.Average(p => p.GetDeliveryRate());
        var avgQuality = performances.Average(p => p.QualityScore);
        var avgManagerRating = (decimal) performances.Average(p => p.ManagerRating) * 20m;
        var avgEstimationAccuracy = performances.Average(p => p.GetEstimationAccuracy());

        // Weighted calculation
        var score = (avgDeliveryRate * 0.35m) +
                   (avgQuality * 0.30m) +
                   (avgManagerRating * 0.25m) +
                   (avgEstimationAccuracy * 0.10m);

        var details = new Dictionary<string, object>
        {
            ["DeliveryRate"] = Math.Round(avgDeliveryRate, 1),
            ["QualityScore"] = Math.Round(avgQuality, 1),
            ["ManagerRating"] = Math.Round(avgManagerRating / 20m, 1), // Back to 1-5 scale
            ["ProjectsEvaluated"] = performances.Count
        };

        return new ComponentScore
        {
            Score = Math.Round(score, 2),
            Explanation = $"Based on {performances.Count} recent projects: {avgDeliveryRate:F0}% on-time delivery, {avgQuality:F0}% quality score",
            Confidence = performances.Count / 3m, // Full confidence with 3 projects
            Details = details
        };
    }
}