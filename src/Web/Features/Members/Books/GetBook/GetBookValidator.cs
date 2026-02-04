using FastEndpoints;
using FluentValidation;

namespace Web.Features.Members.Books.GetBook;

public class GetBookValidator: Validator<GetBookRequest>
{
    public GetBookValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithErrorCode("EmptyBookId")
            .WithMessage("Book id is required.");
    }
}