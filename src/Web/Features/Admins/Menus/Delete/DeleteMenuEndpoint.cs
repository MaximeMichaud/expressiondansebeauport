using Domain.Repositories;
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

    public DeleteMenuEndpoint(INavigationMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
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
        await _menuRepository.Delete(menu);
        await Send.NoContentAsync(ct);
    }
}
