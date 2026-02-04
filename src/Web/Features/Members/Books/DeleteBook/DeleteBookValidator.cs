using FastEndpoints;
using FluentValidation;

namespace Web.Features.Members.Books.DeleteBook;

public class DeleteBookValidator: Validator<DeleteBookRequest>
{
    public DeleteBookValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithErrorCode("EmptyBookId")
            .WithMessage("Book id is required.");
    }
}