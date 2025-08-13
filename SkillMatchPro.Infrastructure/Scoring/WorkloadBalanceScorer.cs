using SkillMatchPro.Application.Scoring;

namespace SkillMatchPro.Infrastructure.Scoring;

public class WorkloadBalanceScorer : IScoringComponent
{
    public string Name => "WorkloadBalance";
    public decimal DefaultWeight => 0.10m;

    public Task<ComponentScore> CalculateScore(ScoringContext context)
    {
        var currentAllocation = context.Candidate.GetCurrentAllocationPercentage();

        // Ideal allocation is 60-80% (room for emergencies, meetings, learning)
        decimal score;
        string explanation;

        if (currentAllocation <= 60)
        {
            score = 100; // Perfect - has good availability
            explanation = $"Ideal workload at {currentAllocation}% allocation";
        }
        else if (currentAllocation <= 80)
        {
            score = 90; // Good - reasonably loaded
            explanation = $"Good workload at {currentAllocation}% allocation";
        }
        else if (currentAllocation < 100)
        {
            score = 60; // OK - getting stretched
            explanation = $"High workload at {currentAllocation}% allocation";
        }
        else
        {
            score = 0; // Cannot take more work
            explanation = "Already at 100% allocation";
        }

        var details = new Dictionary<string, object>
        {
            ["CurrentAllocation"] = currentAllocation,
            ["AvailableCapacity"] = 100 - currentAllocation,
            ["IsOverloaded"] = currentAllocation >= 100
        };

        return Task.FromResult(new ComponentScore
        {
            Score = score,
            Explanation = explanation,
            Confidence = 1.0m, // Always confident about current allocation
            Details = details
        });
    }
}