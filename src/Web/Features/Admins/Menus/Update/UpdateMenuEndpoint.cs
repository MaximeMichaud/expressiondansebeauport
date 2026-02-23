using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Menus.Update;

public class UpdateMenuRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Location { get; set; } = "Primary";
}

public class UpdateMenuValidator : Validator<UpdateMenuRequest>
{
    public UpdateMenuValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().NotEmpty()
            .WithErrorCode("NameRequired")
            .WithMessage("Menu name is required.")
            .MaximumLength(50)
            .WithErrorCode("NameTooLong")
            .WithMessage("Menu name must be 50 characters or less.");
    }
}

public class UpdateMenuEndpoint : Endpoint<UpdateMenuRequest, NavigationMenuDto>
{
    private readonly INavigationMenuRepository _menuRepository;
    private readonly IMapper _mapper;

    public UpdateMenuEndpoint(INavigationMenuRepository menuRepository, IMapper mapper)
    {
        _menuRepository = menuRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Patch("admin/menus/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateMenuRequest req, CancellationToken ct)
    {
        var menu = _menuRepository.FindById(req.Id);
        if (menu is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        menu.SetName(req.Name);
        if (Enum.TryParse<MenuLocation>(req.Location, true, out var loc))
            menu.SetLocation(loc);

        await _menuRepository.Update(menu);
        await Send.OkAsync(_mapper.Map<NavigationMenuDto>(menu), cancellation: ct);
    }
}
