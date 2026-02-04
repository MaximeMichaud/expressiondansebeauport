using FastEndpoints;
using FluentValidation;

namespace Web.Features.Public.Authentication.TwoFactor;

public class TwoFactorValidator : Validator<TwoFactorRequest>
{
    public TwoFactorValidator()
    {
        RuleFor(x => x.Username)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithErrorCode("InvalidUsername")
            .WithMessage("Username should not be empty.");

        RuleFor(x => x.Code)
            .NotNull()
            .NotEmpty()
            .Must(x => x is { Length: 6 } && x.All(char.IsDigit))
            .WithErrorCode("InvalidCode")
            .WithMessage("Code has incorrect format.");
    }
}
