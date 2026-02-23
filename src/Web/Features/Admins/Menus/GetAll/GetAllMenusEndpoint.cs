using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Menus.GetAll;

public class GetAllMenusEndpoint : EndpointWithoutRequest<List<NavigationMenuDto>>
{
    private readonly INavigationMenuRepository _menuRepository;
    private readonly IMapper _mapper;

    public GetAllMenusEndpoint(INavigationMenuRepository menuRepository, IMapper mapper)
    {
        _menuRepository = menuRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/menus");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var menus = _menuRepository.GetAll();
        await Send.OkAsync(_mapper.Map<List<NavigationMenuDto>>(menus), cancellation: ct);
    }
}
