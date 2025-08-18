using SkillMatchPro.Application.Scoring;
using SkillMatchPro.Domain.Enums;

namespace SkillMatchPro.Infrastructure.Scoring;

public class ProficiencyScorer : IScoringComponent
{
    public string Name => "Proficiency";
    public decimal DefaultWeight => 0.30m;

    public Task<ComponentScore> CalculateScore(ScoringContext context)
    {
        var employeeSkill = context.Candidate.EmployeeSkills
            .FirstOrDefault(es => es.SkillId == context.RequiredSkill.Id);

        if (employeeSkill == null)
        {
            return Task.FromResult(new ComponentScore
            {
                Score = 0,
                Explanation = "Employee doesn't have the required skill",
                Confidence = 1.0m
            });
        }

        var actual = employeeSkill.Proficiency;
        var required = context.RequiredProficiency;
        var difference = (int)actual - (int)required;

        decimal score;
        string explanation;

        switch (difference)
        {
            case >= 2:
                score = 100;
                explanation = $"Significantly exceeds requirement ({actual} vs {required})";
                break;
            case 1:
                score = 90;
                explanation = $"Exceeds requirement ({actual} vs {required})";
                break;
            case 0:
                score = 80;
                explanation = $"Meets requirement exactly ({actual})";
                break;
            case -1:
                score = 60;
                explanation = $"Slightly below requirement ({actual} vs {required})";
                break;
            default:
                score = 40;
                explanation = $"Below requirement ({actual} vs {required})";
                break;
        }

        var details = new Dictionary<string, object>
        {
            ["EmployeeProficiency"] = actual.ToString(),
            ["RequiredProficiency"] = required.ToString(),
            ["ProficiencyGap"] = difference
        };

        return Task.FromResult(new ComponentScore
        {
            Score = score,
            Explanation = explanation,
            Confidence = 1.0m,
            Details = details
        });
    }
}