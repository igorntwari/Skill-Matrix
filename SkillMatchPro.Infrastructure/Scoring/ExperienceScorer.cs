using SkillMatchPro.Application.Scoring;

namespace SkillMatchPro.Infrastructure.Scoring;

public class ExperienceScorer : IScoringComponent
{
    public string Name => "Experience";
    public decimal DefaultWeight => 0.05m;

    public Task<ComponentScore> CalculateScore(ScoringContext context)
    {
        var employeeSkill = context.Candidate.EmployeeSkills
            .FirstOrDefault(es => es.SkillId == context.RequiredSkill.Id);

        if (employeeSkill == null)
        {
            return Task.FromResult(new ComponentScore
            {
                Score = 0,
                Explanation = "No experience with this skill",
                Confidence = 1.0m
            });
        }

        // Calculate months of experience - AcquiredDate is NOT nullable
        var experienceMonths = (DateTime.UtcNow - employeeSkill.AcquiredDate).TotalDays / 30.0;
        var lastUsedMonthsAgo = (DateTime.UtcNow - employeeSkill.LastUsedDate)?.TotalDays / 30.0 ?? 0;
        // Base score on experience duration
        decimal experienceScore;
        if (experienceMonths >= 60) // 5+ years
            experienceScore = 100;
        else if (experienceMonths >= 36) // 3+ years
            experienceScore = 85;
        else if (experienceMonths >= 24) // 2+ years
            experienceScore = 70;
        else if (experienceMonths >= 12) // 1+ year
            experienceScore = 55;
        else if (experienceMonths >= 6) // 6+ months
            experienceScore = 40;
        else
            experienceScore = 25;

        // Penalty for not using skill recently
        decimal recencyPenalty = 0;
        if (lastUsedMonthsAgo > 12)
            recencyPenalty = 20;
        else if (lastUsedMonthsAgo > 6)
            recencyPenalty = 10;
        else if (lastUsedMonthsAgo > 3)
            recencyPenalty = 5;

        var finalScore = Math.Max(0, experienceScore - recencyPenalty);

        var yearsExperience = Math.Round(experienceMonths / 12, 1);
        var explanation = $"{yearsExperience} years experience";

        if (recencyPenalty > 0)
            explanation += $", last used {Math.Round(lastUsedMonthsAgo, 0)} months ago";

        var details = new Dictionary<string, object>
        {
            ["ExperienceMonths"] = Math.Round(experienceMonths, 0),
            ["LastUsedMonthsAgo"] = Math.Round(lastUsedMonthsAgo, 0),
            ["YearsExperience"] = yearsExperience,
            ["RecencyPenalty"] = recencyPenalty
        };

        return Task.FromResult(new ComponentScore
        {
            Score = (decimal)finalScore,
            Explanation = explanation,
            Confidence = 1.0m,
            Details = details
        });
    }
}