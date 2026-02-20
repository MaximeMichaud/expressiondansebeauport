using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Menus.Items;

// --- Create Menu Item ---
public class CreateMenuItemRequest
{
    public Guid MenuId { get; set; }
    public Guid? ParentId { get; set; }
    public string Label { get; set; } = null!;
    public string? Url { get; set; }
    public Guid? PageId { get; set; }
    public int SortOrder { get; set; }
    public string Target { get; set; } = "Self";
}

public class CreateMenuItemValidator : Validator<CreateMenuItemRequest>
{
    public CreateMenuItemValidator()
    {
        RuleFor(x => x.Label)
            .NotNull().NotEmpty()
            .WithErrorCode("LabelRequired")
            .WithMessage("Label is required.")
            .MaximumLength(50)
            .WithErrorCode("LabelTooLong")
            .WithMessage("Label must be 50 characters or less.");
    }
}

public class CreateMenuItemEndpoint : Endpoint<CreateMenuItemRequest, NavigationMenuItemDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IMapper _mapper;

    public CreateMenuItemEndpoint(GarneauTemplateDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/menus/{menuId}/items");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateMenuItemRequest req, CancellationToken ct)
    {
        var item = new NavigationMenuItem(req.MenuId, req.Label, req.SortOrder);
        item.SetUrl(req.Url);
        item.SetPageId(req.PageId);
        item.SetParentId(req.ParentId);
        if (Enum.TryParse<MenuItemTarget>(req.Target, true, out var target))
            item.SetTarget(target);

        _context.NavigationMenuItems.Add(item);
        await _context.SaveChangesAsync();
        await Send.OkAsync(_mapper.Map<NavigationMenuItemDto>(item), cancellation: ct);
    }
}

// --- Update Menu Item ---
public class UpdateMenuItemRequest
{
    public Guid MenuId { get; set; }
    public Guid Id { get; set; }
    public string Label { get; set; } = null!;
    public string? Url { get; set; }
    public Guid? PageId { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public string Target { get; set; } = "Self";
}

public class UpdateMenuItemEndpoint : Endpoint<UpdateMenuItemRequest, NavigationMenuItemDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IMapper _mapper;

    public UpdateMenuItemEndpoint(GarneauTemplateDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Patch("admin/menus/{menuId}/items/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateMenuItemRequest req, CancellationToken ct)
    {
        var item = _context.NavigationMenuItems.FirstOrDefault(i => i.Id == req.Id && i.MenuId == req.MenuId);
        if (item is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        item.SetLabel(req.Label);
        item.SetUrl(req.Url);
        item.SetPageId(req.PageId);
        item.SetParentId(req.ParentId);
        item.SetSortOrder(req.SortOrder);
        if (Enum.TryParse<MenuItemTarget>(req.Target, true, out var target))
            item.SetTarget(target);

        _context.NavigationMenuItems.Update(item);
        await _context.SaveChangesAsync();
        await Send.OkAsync(_mapper.Map<NavigationMenuItemDto>(item), cancellation: ct);
    }
}

// --- Delete Menu Item ---
public class DeleteMenuItemRequest
{
    public Guid MenuId { get; set; }
    public Guid Id { get; set; }
}

public class DeleteMenuItemEndpoint : Endpoint<DeleteMenuItemRequest, EmptyResponse>
{
    private readonly GarneauTemplateDbContext _context;

    public DeleteMenuItemEndpoint(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/menus/{menuId}/items/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteMenuItemRequest req, CancellationToken ct)
    {
        var item = _context.NavigationMenuItems.FirstOrDefault(i => i.Id == req.Id && i.MenuId == req.MenuId);
        if (item is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        _context.NavigationMenuItems.Remove(item);
        await _context.SaveChangesAsync();
        await Send.NoContentAsync(ct);
    }
}

// --- Reorder Menu Items ---
public class ReorderMenuItemsRequest
{
    public Guid MenuId { get; set; }
    public List<ReorderItem> Items { get; set; } = new();
}

public class ReorderItem
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}

public class ReorderMenuItemsEndpoint : Endpoint<ReorderMenuItemsRequest, EmptyResponse>
{
    private readonly GarneauTemplateDbContext _context;

    public ReorderMenuItemsEndpoint(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/menus/{menuId}/items/reorder");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ReorderMenuItemsRequest req, CancellationToken ct)
    {
        var menuItems = _context.NavigationMenuItems.Where(i => i.MenuId == req.MenuId).ToList();
        foreach (var reorder in req.Items)
        {
            var item = menuItems.FirstOrDefault(i => i.Id == reorder.Id);
            item?.SetSortOrder(reorder.SortOrder);
        }
        await _context.SaveChangesAsync();
        await Send.NoContentAsync(ct);
    }
}
