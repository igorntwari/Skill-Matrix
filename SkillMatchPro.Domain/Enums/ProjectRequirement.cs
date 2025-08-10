using SkillMatchPro.Domain.Enums;

namespace SkillMatchPro.Domain.Entities;

public class ProjectRequirement
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid SkillId { get; private set; }
    public ProficiencyLevel MinimumProficiency { get; private set; }
    public int RequiredCount { get; private set; }

    // Navigation
    public Project Project { get; private set; } = null!;
    public Skill Skill { get; private set; } = null!;

    private ProjectRequirement() { }

    public ProjectRequirement(Guid projectId, Guid skillId,
        ProficiencyLevel minimumProficiency, int requiredCount)
    {
        if (requiredCount <= 0)
            throw new ArgumentException("Required count must be greater than 0");

        Id = Guid.NewGuid();
        ProjectId = projectId;
        SkillId = skillId;
        MinimumProficiency = minimumProficiency;
        RequiredCount = requiredCount;
    }
}