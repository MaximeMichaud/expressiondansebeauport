using FastEndpoints;
using FluentValidation;

namespace Web.Features.Public.Authentication.ForgotPassword;

public class ForgotPasswordValidator : Validator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Username)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithErrorCode("InvalidUsername")
            .WithMessage("Username should not be empty.");

        RuleFor(x => x.ResetPasswordRelativeUrl)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidResetPasswordRelativeUrl")
            .WithMessage("Reset password relative path should not be empty.");
    }
}