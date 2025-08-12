using SkillMatchPro.Domain.Entities;
using SkillMatchPro.Domain.Enums;

namespace SkillMatchPro.Domain.ValueObjects;

public class SkillMatch
{
    public Skill RequiredSkill { get; }
    public ProficiencyLevel RequiredProficiency { get; }
    public int RequiredCount { get; }
    public List<MatchCandidate> Candidates { get; }

    public SkillMatch(
        Skill requiredSkill,
        ProficiencyLevel requiredProficiency,
        int requiredCount)
    {
        RequiredSkill = requiredSkill ?? throw new ArgumentNullException(nameof(requiredSkill));
        RequiredProficiency = requiredProficiency;
        RequiredCount = requiredCount;
        Candidates = new List<MatchCandidate>();
    }

    public void AddCandidate(MatchCandidate candidate)
    {
        if (candidate.RequiredSkill.Id != RequiredSkill.Id)
            throw new InvalidOperationException("Candidate skill doesn't match required skill");

        Candidates.Add(candidate);
    }

    public List<MatchCandidate> GetTopCandidates()
    {
        return Candidates
            .OrderByDescending(c => c.Score.TotalScore)
            .Take(RequiredCount)
            .ToList();
    }

    public bool IsFulfilled()
    {
        return Candidates.Count(c => c.CanBeAssigned(100)) >= RequiredCount;
    }
}