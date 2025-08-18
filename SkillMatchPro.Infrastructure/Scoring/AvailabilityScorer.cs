using SkillMatchPro.Application.Scoring;
using SkillMatchPro.Application.Services;

namespace SkillMatchPro.Infrastructure.Scoring;

public class AvailabilityScorer : IScoringComponent
{
    private readonly IAllocationService _allocationService;

    public string Name => "Availability";
    public decimal DefaultWeight => 0.20m;

    public AvailabilityScorer()
    {
        // This will be injected when we set up DI properly
        _allocationService = null!;
    }

    public AvailabilityScorer(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    public async Task<ComponentScore> CalculateScore(ScoringContext context)
    {
        var currentAllocation = context.Candidate.GetCurrentAllocationPercentage();

        // Check for conflicts in the project period
        var hasConflict = _allocationService != null
            ? await _allocationService.CheckAllocationConflict(
                context.Candidate.Id,
                100, // Assume full allocation for now
                context.ProjectStartDate,
                context.ProjectEndDate)
            : false;

        if (hasConflict)
        {
            return new ComponentScore
            {
                Score = 0,
                Explanation = "Has scheduling conflicts during project period",
                Confidence = 1.0m,
                Details = new Dictionary<string, object>
                {
                    ["HasConflict"] = true,
                    ["CurrentAllocation"] = currentAllocation
                }
            };
        }

        // Score based on available capacity
        decimal score;
        string explanation;

        var availableCapacity = 100 - currentAllocation;

        if (availableCapacity >= 100)
        {
            score = 100;
            explanation = "Fully available";
        }
        else if (availableCapacity >= 80)
        {
            score = 90;
            explanation = $"{availableCapacity}% available capacity";
        }
        else if (availableCapacity >= 60)
        {
            score = 75;
            explanation = $"{availableCapacity}% available capacity";
        }
        else if (availableCapacity >= 40)
        {
            score = 60;
            explanation = $"Limited availability ({availableCapacity}%)";
        }
        else if (availableCapacity >= 20)
        {
            score = 40;
            explanation = $"Very limited availability ({availableCapacity}%)";
        }
        else
        {
            score = 20;
            explanation = $"Minimal availability ({availableCapacity}%)";
        }

        var details = new Dictionary<string, object>
        {
            ["CurrentAllocation"] = currentAllocation,
            ["AvailableCapacity"] = availableCapacity,
            ["HasConflict"] = false
        };

        return new ComponentScore
        {
            Score = score,
            Explanation = explanation,
            Confidence = 1.0m,
            Details = details
        };
    }
}