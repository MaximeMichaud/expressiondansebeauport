using FastEndpoints;
using FluentValidation;

namespace Web.Features.Social.Groups.Image;

public class SetGroupImageValidator : Validator<SetGroupImageRequest>
{
    public SetGroupImageValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty().WithMessage("GroupId is required.");
        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("L'URL de l'image est requise.")
            .MaximumLength(2048).WithMessage("L'URL ne peut pas dépasser 2048 caractères.")
            .Must(url => url.StartsWith('/')).WithMessage("L'URL doit être une URL relative.");
    }
}
