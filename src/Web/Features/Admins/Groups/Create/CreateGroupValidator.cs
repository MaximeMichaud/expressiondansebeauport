using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Groups.Create;

public class CreateGroupValidator : Validator<CreateGroupRequest>
{
    public CreateGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("NameRequired")
            .WithMessage("Group name is required.")
            .MaximumLength(200)
            .WithErrorCode("NameTooLong")
            .WithMessage("Group name must be 200 characters or less.");

        RuleFor(x => x.Season)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("SeasonRequired")
            .WithMessage("Season is required.")
            .MaximumLength(50)
            .WithErrorCode("SeasonTooLong")
            .WithMessage("Season must be 50 characters or less.");
    }
}
