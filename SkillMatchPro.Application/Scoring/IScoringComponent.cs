using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Domain.Enums;

namespace SkillMatchPro.Application.Scoring;

public interface IScoringComponent
{
    string Name { get; }
    decimal DefaultWeight { get; }
    Task<ComponentScore> CalculateScore(ScoringContext context);
}

public class ScoringContext
{
    public Employee Candidate { get; set; } = null!;
    public Skill RequiredSkill { get; set; } = null!;
    public ProficiencyLevel RequiredProficiency { get; set; }
    public Project Project { get; set; } = null!;
    public List<Employee> CurrentTeamMembers { get; set; } = new();
    public DateTime ProjectStartDate { get; set; }
    public DateTime ProjectEndDate { get; set; }
}

public class ComponentScore
{
    public decimal Score { get; set; } // 0-100
    public string Explanation { get; set; } = string.Empty;
    public decimal Confidence { get; set; } = 1.0m; // 0-1
    public Dictionary<string, object> Details { get; set; } = new();
}