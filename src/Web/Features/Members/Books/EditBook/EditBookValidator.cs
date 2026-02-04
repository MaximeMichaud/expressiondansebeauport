using FastEndpoints;
using FluentValidation;
using Web.Extensions;

namespace Web.Features.Members.Books.EditBook;

public class EditBookValidator:  Validator<EditBookRequest>
{
    public EditBookValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("EmptyBookId")
            .WithMessage("Book id is required.");

        RuleFor(x => x.NameFr)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidFrenchName")
            .WithMessage("Book french name should not be empty or null.");

        RuleFor(x => x.NameEn)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidEnglishName")
            .WithMessage("Book english name should not be empty or null.");

        RuleFor(x => x.DescriptionFr)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidFrenchDescription")
            .WithMessage("Book french description should not be empty or null.");

        RuleFor(x => x.DescriptionEn)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidEnglishDescription")
            .WithMessage("Book english description should not be empty or null.");

        RuleFor(x => x.Isbn)
            .NotNull()
            .NotEmpty()
            .Must(x => x.IsValidIsbn())
            .WithErrorCode("InvalidIsbn")
            .WithMessage("Book isbn should be 17 characters long and contain four hyphens.");

        RuleFor(x => x.Author)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidAuthor")
            .WithMessage("Book author should not be empty or null.");

        RuleFor(x => x.Editor)
            .NotNull()
            .NotEmpty()
            .WithErrorCode("InvalidEditor")
            .WithMessage("Book editor should not be empty or null.");

        RuleFor(x => x.YearOfPublication)
            .NotNull()
            .NotEmpty()
            .Must(x => x > 0)
            .WithErrorCode("InvalidYearOfPublication")
            .WithMessage("Book year of publication should be greater than zero.");

        RuleFor(x => x.NumberOfPages)
            .NotNull()
            .NotEmpty()
            .Must(x => x > 0)
            .WithErrorCode("InvalidNumberOfPages")
            .WithMessage("Book number of pages should be greater than zero.");

        RuleFor(x => x.Price)
            .NotNull()
            .NotEmpty()
            .Must(x => x > 0)
            .WithErrorCode("InvalidPrice")
            .WithMessage("Book price should be greater than zero.");
    }
}