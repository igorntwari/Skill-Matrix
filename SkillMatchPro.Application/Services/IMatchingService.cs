using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Domain.Enums;
using SkillMatchPro.Domain.ValueObjects;

namespace SkillMatchPro.Application.Services;

public interface IMatchingService
{
    Task<TeamComposition> GetTeamComposition(Guid projectId, string requestedBy);
    Task<List<MatchCandidate>> FindCandidatesForSkill(
        Guid skillId,
        ProficiencyLevel requiredProficiency,
        DateTime startDate,
        DateTime endDate,
        int requiredAllocation);
    Task<MatchScore> CalculateMatchScore(
        Employee employee,
        Skill requiredSkill,
        ProficiencyLevel requiredProficiency);
}