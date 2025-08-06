using SkillMatchPro.Domain.Enums;

namespace SkillMatchPro.API.GraphQL.Inputs;

public class AssignSkillInput
{
    public Guid EmployeeId { get; set; }
    public Guid SkillId { get; set; }
    public ProficiencyLevel Proficiency { get; set; }
}