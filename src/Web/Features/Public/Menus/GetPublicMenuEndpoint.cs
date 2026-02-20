using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Public.Menus;

public class GetPublicMenuRequest
{
    public string Location { get; set; } = null!;
}

public class GetPublicMenuEndpoint : Endpoint<GetPublicMenuRequest, NavigationMenuDto>
{
    private readonly INavigationMenuRepository _menuRepository;
    private readonly IMapper _mapper;

    public GetPublicMenuEndpoint(INavigationMenuRepository menuRepository, IMapper mapper)
    {
        _menuRepository = menuRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("public/menus/{location}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetPublicMenuRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<MenuLocation>(req.Location, true, out var location))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var menu = _menuRepository.FindByLocation(location);
        if (menu is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var dto = _mapper.Map<NavigationMenuDto>(menu);
        // Build hierarchy
        var lookup = dto.MenuItems.ToDictionary(i => i.Id);
        var roots = new List<NavigationMenuItemDto>();
        foreach (var item in dto.MenuItems)
        {
            if (item.ParentId.HasValue && lookup.TryGetValue(item.ParentId.Value, out var parent))
                parent.Children.Add(item);
            else
                roots.Add(item);
        }
        dto.MenuItems = roots.OrderBy(i => i.SortOrder).ToList();

        await Send.OkAsync(dto, cancellation: ct);
    }
}
