using Domain.Repositories;
using Application.Interfaces.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Menus.Delete;

public class DeleteMenuRequest
{
    public Guid Id { get; set; }
}

public class DeleteMenuEndpoint : Endpoint<DeleteMenuRequest, EmptyResponse>
{
    private readonly INavigationMenuRepository _menuRepository;
    private readonly IAuditLogService _auditLogService;

    public DeleteMenuEndpoint(INavigationMenuRepository menuRepository, IAuditLogService auditLogService)
    {
        _menuRepository = menuRepository;
        _auditLogService = auditLogService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/menus/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteMenuRequest req, CancellationToken ct)
    {
        var menu = _menuRepository.FindById(req.Id);
        if (menu is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var details = $"Menu '{menu.Name}' supprimé.";
        await _menuRepository.Delete(menu);
        await _auditLogService.LogAsync("delete", "menu", req.Id, details);
        await Send.NoContentAsync(ct);
    }
}
