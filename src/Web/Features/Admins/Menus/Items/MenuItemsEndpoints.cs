using Domain.Entities;
using Domain.Repositories;
using Application.Interfaces.Services;
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
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public CreateMenuItemEndpoint(GarneauTemplateDbContext context, IAuditLogService auditLogService, IMapper mapper)
    {
        _context = context;
        _auditLogService = auditLogService;
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
        if (!await _context.NavigationMenus.AnyAsync(m => m.Id == req.MenuId, ct))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (!await IsValidLinkedPage(req.PageId, ct) || !await IsValidParent(req.MenuId, req.ParentId, null, ct))
        {
            await Send.StringAsync(string.Empty, StatusCodes.Status400BadRequest, cancellation: ct);
            return;
        }

        var item = new NavigationMenuItem(req.MenuId, req.Label, req.SortOrder);
        item.SetUrl(req.Url);
        item.SetPageId(req.PageId);
        item.SetParentId(req.ParentId);
        if (Enum.TryParse<MenuItemTarget>(req.Target, true, out var target))
            item.SetTarget(target);

        _context.NavigationMenuItems.Add(item);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("create", "menu", req.MenuId, $"Item de menu '{item.Label}' ajouté au menu.");
        await Send.OkAsync(_mapper.Map<NavigationMenuItemDto>(item), cancellation: ct);
    }

    private async Task<bool> IsValidLinkedPage(Guid? pageId, CancellationToken ct)
    {
        return pageId is null || await _context.Pages.AnyAsync(p => p.Id == pageId.Value, ct);
    }

    private async Task<bool> IsValidParent(Guid menuId, Guid? parentId, Guid? itemId, CancellationToken ct)
    {
        if (parentId is null) return true;
        if (parentId == itemId) return false;

        var parent = await _context.NavigationMenuItems.FirstOrDefaultAsync(i => i.Id == parentId.Value, ct);
        return parent is not null
            && parent.MenuId == menuId
            && parent.ParentId is null;
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

public class UpdateMenuItemValidator : Validator<UpdateMenuItemRequest>
{
    public UpdateMenuItemValidator()
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

public class UpdateMenuItemEndpoint : Endpoint<UpdateMenuItemRequest, NavigationMenuItemDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public UpdateMenuItemEndpoint(GarneauTemplateDbContext context, IAuditLogService auditLogService, IMapper mapper)
    {
        _context = context;
        _auditLogService = auditLogService;
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
        var item = await _context.NavigationMenuItems.FirstOrDefaultAsync(i => i.Id == req.Id && i.MenuId == req.MenuId, ct);
        if (item is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (!await IsValidLinkedPage(req.PageId, ct)
            || !await IsValidParent(req.MenuId, req.ParentId, req.Id, ct)
            || !await CanMoveUnderParent(item.Id, req.ParentId, ct))
        {
            await Send.StringAsync(string.Empty, StatusCodes.Status400BadRequest, cancellation: ct);
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
        await _auditLogService.LogAsync("update", "menu", req.MenuId, $"Item de menu '{item.Label}' modifié dans le menu.");
        await Send.OkAsync(_mapper.Map<NavigationMenuItemDto>(item), cancellation: ct);
    }

    private async Task<bool> IsValidLinkedPage(Guid? pageId, CancellationToken ct)
    {
        return pageId is null || await _context.Pages.AnyAsync(p => p.Id == pageId.Value, ct);
    }

    private async Task<bool> IsValidParent(Guid menuId, Guid? parentId, Guid itemId, CancellationToken ct)
    {
        if (parentId is null) return true;
        if (parentId == itemId) return false;

        var parent = await _context.NavigationMenuItems.FirstOrDefaultAsync(i => i.Id == parentId.Value, ct);
        return parent is not null
            && parent.MenuId == menuId
            && parent.ParentId is null;
    }

    private async Task<bool> CanMoveUnderParent(Guid itemId, Guid? parentId, CancellationToken ct)
    {
        return parentId is null
            || !await _context.NavigationMenuItems.AnyAsync(i => i.ParentId == itemId, ct);
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
    private readonly IAuditLogService _auditLogService;

    public DeleteMenuItemEndpoint(GarneauTemplateDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
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
        var item = await _context.NavigationMenuItems.FirstOrDefaultAsync(i => i.Id == req.Id && i.MenuId == req.MenuId, ct);
        if (item is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var details = $"Item de menu '{item.Label}' supprimé du menu.";

        var children = await _context.NavigationMenuItems
            .Where(i => i.ParentId == item.Id)
            .ToListAsync(ct);
        if (children.Count > 0)
            _context.NavigationMenuItems.RemoveRange(children);

        _context.NavigationMenuItems.Remove(item);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("delete", "menu", req.MenuId, details);
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
    private readonly IAuditLogService _auditLogService;

    public ReorderMenuItemsEndpoint(GarneauTemplateDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
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
        var menuItems = await _context.NavigationMenuItems.Where(i => i.MenuId == req.MenuId).ToListAsync(ct);
        foreach (var reorder in req.Items)
        {
            var item = menuItems.FirstOrDefault(i => i.Id == reorder.Id);
            item?.SetSortOrder(reorder.SortOrder);
        }
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("update", "menu", req.MenuId, "Ordre des items du menu mis à jour.");
        await Send.NoContentAsync(ct);
    }
}
