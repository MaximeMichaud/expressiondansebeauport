using Domain.Entities;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Web.Dtos;
using Application.Interfaces.Services;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.SiteSettings.SocialLinks;

// --- Create ---
public class CreateSocialLinkRequest
{
    public string Platform { get; set; } = null!;
    public string Url { get; set; } = null!;
}

public class CreateSocialLinkValidator : Validator<CreateSocialLinkRequest>
{
    public CreateSocialLinkValidator()
    {
        RuleFor(x => x.Platform).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Url).NotEmpty().MaximumLength(500)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("URL must start with http:// or https://.");
    }
}

public class CreateSocialLinkEndpoint : Endpoint<CreateSocialLinkRequest, SocialLinkDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public CreateSocialLinkEndpoint(GarneauTemplateDbContext context, IAuditLogService auditLogService, IMapper mapper)
    {
        _context = context;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/site-settings/social-links");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateSocialLinkRequest req, CancellationToken ct)
    {
        var settings = await _context.SiteSettings.FirstAsync(ct);
        var maxOrder = await _context.SocialLinks
            .Where(s => s.SiteSettingsId == settings.Id)
            .MaxAsync(s => (int?)s.SortOrder, ct) ?? -1;

        var link = new SocialLink(settings.Id, req.Platform, req.Url, maxOrder + 1);
        _context.SocialLinks.Add(link);
        await _context.SaveChangesAsync(ct);
        await _auditLogService.LogAsync("update", "site-settings", settings.Id, $"Social link '{link.Platform}' added (item: {link.Id}).");
        await Send.OkAsync(_mapper.Map<SocialLinkDto>(link), cancellation: ct);
    }
}

// --- Update ---
public class UpdateSocialLinkRequest
{
    public Guid Id { get; set; }
    public string Platform { get; set; } = null!;
    public string Url { get; set; } = null!;
}

public class UpdateSocialLinkValidator : Validator<UpdateSocialLinkRequest>
{
    public UpdateSocialLinkValidator()
    {
        RuleFor(x => x.Platform).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Url).NotEmpty().MaximumLength(500)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("URL must start with http:// or https://.");
    }
}

public class UpdateSocialLinkEndpoint : Endpoint<UpdateSocialLinkRequest, SocialLinkDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public UpdateSocialLinkEndpoint(GarneauTemplateDbContext context, IAuditLogService auditLogService, IMapper mapper)
    {
        _context = context;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Patch("admin/site-settings/social-links/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateSocialLinkRequest req, CancellationToken ct)
    {
        var link = await _context.SocialLinks.FirstOrDefaultAsync(s => s.Id == req.Id, ct);
        if (link is null) { await Send.NotFoundAsync(ct); return; }

        link.SetPlatform(req.Platform);
        link.SetUrl(req.Url);
        await _context.SaveChangesAsync(ct);
        await _auditLogService.LogAsync("update", "site-settings", link.SiteSettingsId, $"Social link '{link.Platform}' updated (item: {link.Id}).");
        await Send.OkAsync(_mapper.Map<SocialLinkDto>(link), cancellation: ct);
    }
}

// --- Delete ---
public class DeleteSocialLinkRequest
{
    public Guid Id { get; set; }
}

public class DeleteSocialLinkEndpoint : Endpoint<DeleteSocialLinkRequest, EmptyResponse>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public DeleteSocialLinkEndpoint(GarneauTemplateDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/site-settings/social-links/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteSocialLinkRequest req, CancellationToken ct)
    {
        var link = await _context.SocialLinks.FirstOrDefaultAsync(s => s.Id == req.Id, ct);
        if (link is null) { await Send.NotFoundAsync(ct); return; }

        var details = $"Social link '{link.Platform}' deleted.";
        _context.SocialLinks.Remove(link);
        await _context.SaveChangesAsync(ct);
        await _auditLogService.LogAsync("update", "site-settings", link.SiteSettingsId, $"{details} (item: {link.Id}).");
        await Send.NoContentAsync(ct);
    }
}
