using Domain.Entities;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Web.Dtos;
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
        RuleFor(x => x.Url).NotEmpty().MaximumLength(500);
    }
}

public class CreateSocialLinkEndpoint : Endpoint<CreateSocialLinkRequest, SocialLinkDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IMapper _mapper;

    public CreateSocialLinkEndpoint(GarneauTemplateDbContext context, IMapper mapper)
    {
        _context = context;
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
        RuleFor(x => x.Url).NotEmpty().MaximumLength(500);
    }
}

public class UpdateSocialLinkEndpoint : Endpoint<UpdateSocialLinkRequest, SocialLinkDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IMapper _mapper;

    public UpdateSocialLinkEndpoint(GarneauTemplateDbContext context, IMapper mapper)
    {
        _context = context;
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

    public DeleteSocialLinkEndpoint(GarneauTemplateDbContext context)
    {
        _context = context;
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

        _context.SocialLinks.Remove(link);
        await _context.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}
