using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Application.Scoring;
using SkillMatchPro.Domain.Enums;

namespace SkillMatchPro.Application.Services;

public interface IAdvancedMatchingService
{
    Task<AdvancedMatchResult> GetTeamCompositionWithScoring(
        Guid projectId,
        string requestedBy,
        ScoringConfiguration? customWeights = null);

    Task<List<ScoredCandidate>> GetScoredCandidatesForSkill(
        Guid skillId,
        ProficiencyLevel requiredProficiency,
        Project project,
        List<Employee> currentTeam,
        ScoringConfiguration? customWeights = null);
}

public class AdvancedMatchResult
{
    public TeamComposition TeamComposition { get; set; } = null!;
    public List<TeamMemberScore> MemberScores { get; set; } = new();
    public Dictionary<string, string> Recommendations { get; set; } = new();
    public List<string> Risks { get; set; } = new();
}

public class TeamMemberScore
{
    public Employee Employee { get; set; } = null!;
    public decimal TotalScore { get; set; }
    public Dictionary<string, ComponentScore> ComponentScores { get; set; } = new();
}

public class ScoredCandidate
{
    public Employee Employee { get; set; } = null!;
    public decimal TotalScore { get; set; }
    public Dictionary<string, ComponentScore> ComponentScores { get; set; } = new();
    public bool IsAvailable { get; set; }
    public int CurrentAllocation { get; set; }
}
