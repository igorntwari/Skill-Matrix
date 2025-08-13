namespace SkillMatchPro.Application.Scoring;

public class ScoringConfiguration
{
    public Dictionary<string, decimal> ComponentWeights { get; set; } = new();

    public ScoringConfiguration()
    {
        // Default weights
        ComponentWeights["Proficiency"] = 0.30m;
        ComponentWeights["Availability"] = 0.20m;
        ComponentWeights["Performance"] = 0.20m;
        ComponentWeights["TeamChemistry"] = 0.15m;
        ComponentWeights["WorkloadBalance"] = 0.10m;
        ComponentWeights["Experience"] = 0.05m;
    }

    public void ValidateWeights()
    {
        var total = ComponentWeights.Values.Sum();
        if (Math.Abs(total - 1.0m) > 0.01m)
            throw new InvalidOperationException($"Weights must sum to 1.0, current sum: {total}");
    }
}