using FluentValidation;
using SkillMatchPro.API.GraphQL.Inputs;
using SkillMatchPro.Domain.Enums;

namespace SkillMatchPro.API.GraphQL.Validators;

public class AssignSkillInputValidator : AbstractValidator<AssignSkillInput>
{
    public AssignSkillInputValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID is required");

        RuleFor(x => x.SkillId)
            .NotEmpty().WithMessage("Skill ID is required");

        RuleFor(x => x.Proficiency)
            .IsInEnum().WithMessage("Invalid proficiency level")
            .Must(p => p >= ProficiencyLevel.Beginner && p <= ProficiencyLevel.Master)
            .WithMessage("Proficiency must be between 1 and 5");
    }
}