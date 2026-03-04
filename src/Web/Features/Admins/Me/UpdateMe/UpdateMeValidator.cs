using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Me.UpdateMe;

public class UpdateMeValidator : Validator<UpdateMeRequest>
{
    public UpdateMeValidator()
    {
        RuleFor(x => x.FirstName)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidFirstName")
            .WithMessage("First name should not be empty.")
            .MaximumLength(100)
            .WithErrorCode("FirstNameTooLong")
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidLastName")
            .WithMessage("Last name should not be empty.")
            .MaximumLength(100)
            .WithErrorCode("LastNameTooLong")
            .WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidEmail")
            .WithMessage("Email should not be empty.")
            .MaximumLength(320)
            .WithErrorCode("EmailTooLong")
            .WithMessage("Email must not exceed 320 characters.")
            .EmailAddress()
            .WithErrorCode("InvalidEmailFormat")
            .WithMessage("Email format is invalid.");
    }
}
