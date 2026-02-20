using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Pages.Delete;

public class DeletePageRequest
{
    public Guid Id { get; set; }
}

public class DeletePageEndpoint : Endpoint<DeletePageRequest, EmptyResponse>
{
    private readonly IPageRepository _pageRepository;

    public DeletePageEndpoint(IPageRepository pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/pages/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeletePageRequest req, CancellationToken ct)
    {
        var page = _pageRepository.FindById(req.Id);
        if (page is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await _pageRepository.Delete(page);
        await Send.NoContentAsync(ct);
    }
}
