namespace SkillMatchPro.Domain.ValueObjects;

public class MatchScore
{
    public decimal TotalScore { get; }
    public decimal ProficiencyScore { get; }
    public decimal AvailabilityScore { get; }
    public decimal ExperienceScore { get; }
    public string Explanation { get; }

    public MatchScore(
        decimal proficiencyScore,
        decimal availabilityScore,
        decimal experienceScore,
        string explanation = "")
    {
        // Validate scores are between 0 and 100
        if (proficiencyScore < 0 || proficiencyScore > 100)
            throw new ArgumentException("Proficiency score must be between 0 and 100");
        if (availabilityScore < 0 || availabilityScore > 100)
            throw new ArgumentException("Availability score must be between 0 and 100");
        if (experienceScore < 0 || experienceScore > 100)
            throw new ArgumentException("Experience score must be between 0 and 100");

        ProficiencyScore = proficiencyScore;
        AvailabilityScore = availabilityScore;
        ExperienceScore = experienceScore;

        // Calculate weighted total (customize weights as needed)
        TotalScore = (ProficiencyScore * 0.4m) +
                     (AvailabilityScore * 0.3m) +
                     (ExperienceScore * 0.3m);

        Explanation = explanation;
    }

    public bool IsBetterThan(MatchScore other)
    {
        return TotalScore > other.TotalScore;
    }
}