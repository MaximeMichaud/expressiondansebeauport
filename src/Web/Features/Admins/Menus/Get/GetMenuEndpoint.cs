using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Menus.Get;

public class GetMenuRequest
{
    public Guid Id { get; set; }
}

public class GetMenuEndpoint : Endpoint<GetMenuRequest, NavigationMenuDto>
{
    private readonly INavigationMenuRepository _menuRepository;
    private readonly IMapper _mapper;

    public GetMenuEndpoint(INavigationMenuRepository menuRepository, IMapper mapper)
    {
        _menuRepository = menuRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/menus/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetMenuRequest req, CancellationToken ct)
    {
        var menu = _menuRepository.FindById(req.Id, includeItems: true);
        if (menu is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var dto = _mapper.Map<NavigationMenuDto>(menu);
        // Return a flat list so the admin template can use its own filtering
        // (currentMenu.menuItems.filter(i => i.parentId === item.id)).
        // We clear Children to avoid duplicates from EF's relationship fix-up.
        foreach (var item in dto.MenuItems)
            item.Children.Clear();

        await Send.OkAsync(dto, cancellation: ct);
    }
}
