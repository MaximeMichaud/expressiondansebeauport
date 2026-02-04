using Domain.Helpers;
using FastEndpoints;
using FluentValidation;

namespace Web.Features.Admins.Members.CreateMember;

public class CreateMemberValidator : Validator<CreateMemberRequest>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.FirstName)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidFirstName")
            .WithMessage("First name should not be empty.");

        RuleFor(x => x.LastName)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidLastName")
            .WithMessage("Last name should not be empty.");

        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidEmail")
            .WithMessage("Last name should not be empty.")
            .EmailAddress()
            .WithErrorCode("InvalidEmailFormat")
            .WithMessage("Email format is invalid.");

        RuleFor(x => x.PhoneNumber)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidPhoneNumber")
            .WithMessage("Phone number should not be empty.")
            .Must(x => x.HasValidPhoneNumberFormat())
            .WithErrorCode("InvalidPhoneNumberFormat")
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.Street)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidStreet")
            .WithMessage("Street should not be empty.");

        RuleFor(x => x.City)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidCity")
            .WithMessage("City should not be empty.");

        RuleFor(x => x.ZipCode)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidZipCode")
            .WithMessage("Zip code should not be empty.")
            .Must(x => x.HasCanadianZipCodeFormat())
            .WithErrorCode("InvalidZipCodeFormat")
            .WithMessage("Zip code address format is not valid.");
    }
}