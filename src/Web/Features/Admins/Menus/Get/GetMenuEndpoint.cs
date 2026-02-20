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
        // Build hierarchy - only return top-level items with children nested
        dto.MenuItems = BuildHierarchy(dto.MenuItems);
        await Send.OkAsync(dto, cancellation: ct);
    }

    private static List<NavigationMenuItemDto> BuildHierarchy(List<NavigationMenuItemDto> items)
    {
        var lookup = items.ToDictionary(i => i.Id);
        var roots = new List<NavigationMenuItemDto>();
        foreach (var item in items)
        {
            if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
                parent.Children.Add(item);
            else
                roots.Add(item);
        }
        return roots.OrderBy(i => i.SortOrder).ToList();
    }
}
