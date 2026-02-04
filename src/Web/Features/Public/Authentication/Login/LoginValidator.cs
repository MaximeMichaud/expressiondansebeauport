using FastEndpoints;
using FluentValidation;

namespace Web.Features.Public.Authentication.Login;

public class LoginValidator : Validator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithErrorCode("InvalidUsername")
            .WithMessage("Username should not be empty.");
    }
}
