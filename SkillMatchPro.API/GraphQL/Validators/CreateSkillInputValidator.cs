using FluentValidation;
using SkillMatchPro.API.GraphQL.Inputs;

namespace SkillMatchPro.API.GraphQL.Validators;

public class CreateSkillInputValidator : AbstractValidator<CreateSkillInput>
{
    public CreateSkillInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Skill name is required")
            .MaximumLength(100).WithMessage("Skill name cannot exceed 100 characters");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}