using FastEndpoints;
using FluentValidation;

namespace Web.Features.Users.Me.ChangePassword;

public class ChangePasswordValidator : Validator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidCurrentPassword")
            .WithMessage("Current password should not be null or empty.");

        RuleFor(x => x.NewPassword)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidNewPassword")
            .WithMessage("New password should not be null or empty.");

        RuleFor(x => x.NewPasswordConfirmation)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidNewPasswordConfirmation")
            .WithMessage("New password confirmation should not be null or empty.")
            .Equal(x => x.NewPassword)
            .WithErrorCode("PasswordAndConfirmationMustMatch")
            .WithMessage("The new password and its confirmation must match.");
    }
}
