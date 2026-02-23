using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Menus.Create;

public class CreateMenuRequest
{
    public string Name { get; set; } = null!;
    public string Location { get; set; } = "Primary";
}

public class CreateMenuValidator : Validator<CreateMenuRequest>
{
    public CreateMenuValidator()
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

public class CreateMenuEndpoint : Endpoint<CreateMenuRequest, NavigationMenuDto>
{
    private readonly INavigationMenuRepository _menuRepository;
    private readonly IMapper _mapper;

    public CreateMenuEndpoint(INavigationMenuRepository menuRepository, IMapper mapper)
    {
        _menuRepository = menuRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/menus");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateMenuRequest req, CancellationToken ct)
    {
        var location = Enum.TryParse<MenuLocation>(req.Location, true, out var loc) ? loc : MenuLocation.Primary;
        var menu = new NavigationMenu(req.Name, location);
        await _menuRepository.Create(menu);
        await Send.OkAsync(_mapper.Map<NavigationMenuDto>(menu), cancellation: ct);
    }
}
