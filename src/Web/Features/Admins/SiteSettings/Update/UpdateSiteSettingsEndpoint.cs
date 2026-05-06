using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using Application.Interfaces.Services;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.SiteSettings.Update;

public class UpdateSiteSettingsRequest
{
    public string SiteTitle { get; set; } = null!;
    public string? Tagline { get; set; }
    public string PrimaryColor { get; set; } = null!;
    public string? SecondaryColor { get; set; }
    public Guid? LogoMediaFileId { get; set; }
    public Guid? FaviconMediaFileId { get; set; }
    public string HeadingFont { get; set; } = null!;
    public string BodyFont { get; set; } = null!;
    public string? FooterDescription { get; set; }
    public string? FooterAddress { get; set; }
    public string? FooterCity { get; set; }
    public string? FooterPhone { get; set; }
    public string? FooterEmail { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? CopyrightText { get; set; }
    public bool IsMaintenanceMode { get; set; }
    public string MaintenanceMessage { get; set; } = "Le site est en maintenance. Revenez bientôt !";
    public int MaintenanceRetryAfter { get; set; } = 3600;
    public int AuditLogRetentionDays { get; set; } = 90;
    public bool IsBannerEnabled { get; set; }
    public string BannerText { get; set; } = "Actualités du moment";
    public string BannerUrl { get; set; } = "/actualites";
}

public class UpdateSiteSettingsValidator : Validator<UpdateSiteSettingsRequest>
{
    public UpdateSiteSettingsValidator()
    {
        RuleFor(x => x.SiteTitle)
            .NotNull().NotEmpty()
            .WithErrorCode("SiteTitleRequired")
            .WithMessage("Site title is required.")
            .MaximumLength(100)
            .WithErrorCode("SiteTitleTooLong")
            .WithMessage("Site title must be 100 characters or less.");

        RuleFor(x => x.PrimaryColor)
            .NotNull().NotEmpty()
            .WithErrorCode("PrimaryColorRequired")
            .WithMessage("Primary color is required.")
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .WithErrorCode("InvalidColorFormat")
            .WithMessage("Color must be in hex format (#RRGGBB).");

        RuleFor(x => x.SecondaryColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => x.SecondaryColor is not null)
            .WithErrorCode("InvalidColorFormat")
            .WithMessage("Color must be in hex format (#RRGGBB).");

        RuleFor(x => x.HeadingFont)
            .NotNull().NotEmpty()
            .WithErrorCode("HeadingFontRequired")
            .WithMessage("Heading font is required.");

        RuleFor(x => x.BodyFont)
            .NotNull().NotEmpty()
            .WithErrorCode("BodyFontRequired")
            .WithMessage("Body font is required.");

        RuleFor(x => x.FooterDescription).MaximumLength(500);
        RuleFor(x => x.FooterAddress).MaximumLength(200);
        RuleFor(x => x.FooterCity).MaximumLength(100);
        RuleFor(x => x.FooterPhone).MaximumLength(20);
        RuleFor(x => x.FooterEmail).MaximumLength(100).EmailAddress().When(x => !string.IsNullOrEmpty(x.FooterEmail));
        RuleFor(x => x.FacebookUrl).MaximumLength(500);
        RuleFor(x => x.InstagramUrl).MaximumLength(500);
        RuleFor(x => x.CopyrightText).MaximumLength(200);
        RuleFor(x => x.AuditLogRetentionDays).InclusiveBetween(1, 3650);
    }
}

public class UpdateSiteSettingsEndpoint : Endpoint<UpdateSiteSettingsRequest, SiteSettingsDto>
{
    private readonly ISiteSettingsRepository _settingsRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;

    public UpdateSiteSettingsEndpoint(ISiteSettingsRepository settingsRepository, IAuditLogService auditLogService, IMapper mapper)
    {
        _settingsRepository = settingsRepository;
        _auditLogService = auditLogService;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("admin/site-settings");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateSiteSettingsRequest req, CancellationToken ct)
    {
        var settings = await _settingsRepository.Get();
        var changedFields = GetChangedFields(settings, req);
        settings.SetSiteTitle(req.SiteTitle);
        settings.SetTagline(req.Tagline);
        settings.SetPrimaryColor(req.PrimaryColor);
        settings.SetSecondaryColor(req.SecondaryColor);
        settings.SetLogoMediaFileId(req.LogoMediaFileId);
        settings.SetFaviconMediaFileId(req.FaviconMediaFileId);
        settings.SetHeadingFont(req.HeadingFont);
        settings.SetBodyFont(req.BodyFont);
        settings.SetFooterDescription(req.FooterDescription);
        settings.SetFooterAddress(req.FooterAddress);
        settings.SetFooterCity(req.FooterCity);
        settings.SetFooterPhone(req.FooterPhone);
        settings.SetFooterEmail(req.FooterEmail);
        settings.SetFacebookUrl(req.FacebookUrl);
        settings.SetInstagramUrl(req.InstagramUrl);
        settings.SetCopyrightText(req.CopyrightText);
        settings.SetMaintenanceMode(req.IsMaintenanceMode);
        settings.SetMaintenanceMessage(req.MaintenanceMessage);
        settings.SetMaintenanceRetryAfter(req.MaintenanceRetryAfter);
        settings.SetAuditLogRetentionDays(req.AuditLogRetentionDays);
        settings.SetBannerEnabled(req.IsBannerEnabled);
        settings.SetBannerText(req.BannerText);
        settings.SetBannerUrl(req.BannerUrl);

        await _settingsRepository.Update(settings);
        if (changedFields.Count > 0)
        {
            await _auditLogService.LogAsync(
                "update",
                "site-settings",
                settings.Id,
                $"Site settings updated: {string.Join(", ", changedFields)}.");
        }
        await Send.OkAsync(_mapper.Map<SiteSettingsDto>(settings), cancellation: ct);
    }

    private static List<string> GetChangedFields(Domain.Entities.SiteSettings settings, UpdateSiteSettingsRequest req)
    {
        var changedFields = new List<string>();
        if (settings.SiteTitle != req.SiteTitle) changedFields.Add("site title");
        if (settings.Tagline != req.Tagline) changedFields.Add("tagline");
        if (settings.PrimaryColor != req.PrimaryColor) changedFields.Add("primary color");
        if (settings.SecondaryColor != req.SecondaryColor) changedFields.Add("secondary color");
        if (settings.LogoMediaFileId != req.LogoMediaFileId) changedFields.Add("logo");
        if (settings.FaviconMediaFileId != req.FaviconMediaFileId) changedFields.Add("favicon");
        if (settings.HeadingFont != req.HeadingFont) changedFields.Add("heading font");
        if (settings.BodyFont != req.BodyFont) changedFields.Add("body font");
        if (settings.FooterDescription != req.FooterDescription) changedFields.Add("footer description");
        if (settings.FooterAddress != req.FooterAddress) changedFields.Add("footer address");
        if (settings.FooterCity != req.FooterCity) changedFields.Add("footer city");
        if (settings.FooterPhone != req.FooterPhone) changedFields.Add("footer phone");
        if (settings.FooterEmail != req.FooterEmail) changedFields.Add("footer email");
        if (settings.FacebookUrl != req.FacebookUrl) changedFields.Add("facebook url");
        if (settings.InstagramUrl != req.InstagramUrl) changedFields.Add("instagram url");
        if (settings.CopyrightText != req.CopyrightText) changedFields.Add("copyright text");
        if (settings.IsMaintenanceMode != req.IsMaintenanceMode) changedFields.Add("maintenance mode");
        if (settings.MaintenanceMessage != req.MaintenanceMessage) changedFields.Add("maintenance message");
        if (settings.MaintenanceRetryAfter != req.MaintenanceRetryAfter) changedFields.Add("maintenance retry after");
        if (settings.AuditLogRetentionDays != req.AuditLogRetentionDays) changedFields.Add("audit log retention");
        return changedFields;
    }
}
