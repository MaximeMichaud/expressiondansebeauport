using Domain.Entities;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.SiteSettings.Partners;

// --- Create ---
public class CreateFooterPartnerRequest
{
    public Guid MediaFileId { get; set; }
    public string AltText { get; set; } = null!;
    public string? Url { get; set; }
}

public class CreateFooterPartnerValidator : Validator<CreateFooterPartnerRequest>
{
    public CreateFooterPartnerValidator()
    {
        RuleFor(x => x.MediaFileId).NotEmpty();
        RuleFor(x => x.AltText).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Url).MaximumLength(500)
            .Must(url => url is null || Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("URL must start with http:// or https://.");
    }
}

public class CreateFooterPartnerEndpoint : Endpoint<CreateFooterPartnerRequest, FooterPartnerDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IMapper _mapper;

    public CreateFooterPartnerEndpoint(GarneauTemplateDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/site-settings/partners");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateFooterPartnerRequest req, CancellationToken ct)
    {
        var mediaExists = await _context.MediaFiles.AnyAsync(m => m.Id == req.MediaFileId, ct);
        if (!mediaExists) { await Send.NotFoundAsync(ct); return; }

        var settings = await _context.SiteSettings.FirstAsync(ct);
        var maxOrder = await _context.FooterPartners
            .Where(p => p.SiteSettingsId == settings.Id)
            .MaxAsync(p => (int?)p.SortOrder, ct) ?? -1;

        var partner = new FooterPartner(settings.Id, req.MediaFileId, req.AltText, req.Url, maxOrder + 1);
        _context.FooterPartners.Add(partner);
        await _context.SaveChangesAsync(ct);

        var created = await _context.FooterPartners
            .Include(p => p.MediaFile)
            .FirstAsync(p => p.Id == partner.Id, ct);
        await Send.OkAsync(_mapper.Map<FooterPartnerDto>(created), cancellation: ct);
    }
}

// --- Update ---
public class UpdateFooterPartnerRequest
{
    public Guid Id { get; set; }
    public Guid MediaFileId { get; set; }
    public string AltText { get; set; } = null!;
    public string? Url { get; set; }
}

public class UpdateFooterPartnerValidator : Validator<UpdateFooterPartnerRequest>
{
    public UpdateFooterPartnerValidator()
    {
        RuleFor(x => x.MediaFileId).NotEmpty();
        RuleFor(x => x.AltText).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Url).MaximumLength(500)
            .Must(url => url is null || Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("URL must start with http:// or https://.");
    }
}

public class UpdateFooterPartnerEndpoint : Endpoint<UpdateFooterPartnerRequest, FooterPartnerDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IMapper _mapper;

    public UpdateFooterPartnerEndpoint(GarneauTemplateDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Patch("admin/site-settings/partners/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateFooterPartnerRequest req, CancellationToken ct)
    {
        var partner = await _context.FooterPartners.FirstOrDefaultAsync(p => p.Id == req.Id, ct);
        if (partner is null) { await Send.NotFoundAsync(ct); return; }

        var mediaExists = await _context.MediaFiles.AnyAsync(m => m.Id == req.MediaFileId, ct);
        if (!mediaExists) { await Send.NotFoundAsync(ct); return; }

        partner.SetMediaFileId(req.MediaFileId);
        partner.SetAltText(req.AltText);
        partner.SetUrl(req.Url);
        await _context.SaveChangesAsync(ct);

        var updated = await _context.FooterPartners
            .Include(p => p.MediaFile)
            .FirstAsync(p => p.Id == partner.Id, ct);
        await Send.OkAsync(_mapper.Map<FooterPartnerDto>(updated), cancellation: ct);
    }
}

// --- Delete ---
public class DeleteFooterPartnerRequest
{
    public Guid Id { get; set; }
}

public class DeleteFooterPartnerEndpoint : Endpoint<DeleteFooterPartnerRequest, EmptyResponse>
{
    private readonly GarneauTemplateDbContext _context;

    public DeleteFooterPartnerEndpoint(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/site-settings/partners/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteFooterPartnerRequest req, CancellationToken ct)
    {
        var partner = await _context.FooterPartners.FirstOrDefaultAsync(p => p.Id == req.Id, ct);
        if (partner is null) { await Send.NotFoundAsync(ct); return; }

        _context.FooterPartners.Remove(partner);
        await _context.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}
