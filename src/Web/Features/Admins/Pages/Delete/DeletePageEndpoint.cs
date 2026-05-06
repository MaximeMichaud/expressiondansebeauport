using Domain.Repositories;
using Application.Interfaces.Services;
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
    private readonly IAuditLogService _auditLogService;

    public DeletePageEndpoint(IPageRepository pageRepository, IAuditLogService auditLogService)
    {
        _pageRepository = pageRepository;
        _auditLogService = auditLogService;
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
        var details = $"Page '{page.Title}' supprimée.";
        await _pageRepository.Delete(page);
        await _auditLogService.LogAsync("delete", "page", req.Id, details);
        await Send.NoContentAsync(ct);
    }
}
