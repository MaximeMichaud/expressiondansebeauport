using FastEndpoints;
using FluentValidation;

namespace Web.Features.Social.Members.ProfileImage;

public class SetProfileImageValidator : Validator<SetProfileImageRequest>
{
    public SetProfileImageValidator()
    {
        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("L'URL de l'image est requise.")
            .MaximumLength(2048).WithMessage("L'URL ne peut pas dépasser 2048 caractères.")
            .Must(url => url.StartsWith('/')).WithMessage("L'URL doit être une URL relative.");
    }
}
