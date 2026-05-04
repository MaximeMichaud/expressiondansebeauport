using FastEndpoints;
using FluentValidation;

namespace Web.Features.Social.Groups.Polls;

public class CreatePollValidator : Validator<CreatePollRequest>
{
    public CreatePollValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("GroupId is required.");

        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("La question est requise.")
            .MaximumLength(500).WithMessage("La question ne peut pas dépasser 500 caractères.");

        RuleFor(x => x.Options)
            .NotNull()
            .Must(opts => opts.Count >= 2).WithMessage("Au moins 2 options sont requises.")
            .Must(opts => opts.Count <= 10).WithMessage("Maximum 10 options.")
            .Must(opts => opts.All(o => !string.IsNullOrWhiteSpace(o)))
                .WithMessage("Aucune option ne peut être vide.")
            .Must(opts => opts.All(o => o.Length <= 200))
                .WithMessage("Chaque option ne peut pas dépasser 200 caractères.")
            .Must(opts => opts
                .Select(o => o.Trim().ToLowerInvariant())
                .Distinct()
                .Count() == opts.Count)
                .WithMessage("Les options doivent être uniques.");
    }
}
