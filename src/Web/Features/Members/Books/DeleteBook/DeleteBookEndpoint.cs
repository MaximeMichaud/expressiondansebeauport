using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Members.Books.DeleteBook;

public class DeleteBookEndpoint: Endpoint<DeleteBookRequest, EmptyResponse>
{
    private readonly IBookRepository _bookRepository;

    public DeleteBookEndpoint(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Delete("books/{id}");
        Roles(Domain.Constants.User.Roles.MEMBER);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteBookRequest request, CancellationToken ct)
    {
        await _bookRepository.DeleteBookWithId(request.Id);
        await Send.NoContentAsync(ct);
    }
}