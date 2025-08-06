using SkillMatchPro.Domain.Enums;

namespace SkillMatchPro.Domain.Entities;

public class EmployeeSkill
{
    public Guid EmployeeId { get; private set; }
    public Guid SkillId { get; private set; }
    public ProficiencyLevel Proficiency { get; private set; }
    public DateTime AcquiredDate { get; private set; }
    public DateTime? LastUsedDate { get; private set; }

    public Employee Employee { get; private set; }
    public Skill Skill { get; private set; }

    private EmployeeSkill() { }

    public EmployeeSkill(Guid employeeId, Guid skillId, ProficiencyLevel proficiency)
    {
        EmployeeId = employeeId;
        SkillId = skillId;
        Proficiency = proficiency;
        AcquiredDate = DateTime.UtcNow;
    }

    public void UpdateProficiency(ProficiencyLevel newLevel)
    {
        Proficiency = newLevel;
        LastUsedDate = DateTime.UtcNow;
    }
}