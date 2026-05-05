using FastEndpoints;
using FluentValidation;

namespace Web.Features.Social.Groups.JoinRequests.Create;

public class CreateJoinRequestValidator : Validator<CreateJoinRequestRequest>
{
    public CreateJoinRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("GroupId is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Une raison est requise.")
            .MaximumLength(200).WithMessage("La raison ne peut pas dépasser 200 caractères.");
    }
}
