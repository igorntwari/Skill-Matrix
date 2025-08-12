using SkillMatchPro.Domain.Entities;

namespace SkillMatchPro.Domain.ValueObjects;

public class MatchCandidate
{
    public Employee Employee { get; }
    public Skill RequiredSkill { get; }
    public MatchScore Score { get; }
    public bool IsAvailable { get; }
    public int CurrentAllocation { get; }

    public MatchCandidate(
        Employee employee,
        Skill requiredSkill,
        MatchScore score,
        bool isAvailable,
        int currentAllocation)
    {
        Employee = employee ?? throw new ArgumentNullException(nameof(employee));
        RequiredSkill = requiredSkill ?? throw new ArgumentNullException(nameof(requiredSkill));
        Score = score ?? throw new ArgumentNullException(nameof(score));
        IsAvailable = isAvailable;
        CurrentAllocation = currentAllocation;
    }

    public bool CanBeAssigned(int requiredAllocation)
    {
        return IsAvailable && (CurrentAllocation + requiredAllocation <= 100);
    }
}