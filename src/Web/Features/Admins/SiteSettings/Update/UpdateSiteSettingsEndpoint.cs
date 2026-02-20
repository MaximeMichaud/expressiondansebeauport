using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
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
    }
}

public class UpdateSiteSettingsEndpoint : Endpoint<UpdateSiteSettingsRequest, SiteSettingsDto>
{
    private readonly ISiteSettingsRepository _settingsRepository;
    private readonly IMapper _mapper;

    public UpdateSiteSettingsEndpoint(ISiteSettingsRepository settingsRepository, IMapper mapper)
    {
        _settingsRepository = settingsRepository;
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
        var settings = _settingsRepository.Get();
        settings.SetSiteTitle(req.SiteTitle);
        settings.SetTagline(req.Tagline);
        settings.SetPrimaryColor(req.PrimaryColor);
        settings.SetSecondaryColor(req.SecondaryColor);
        settings.SetLogoMediaFileId(req.LogoMediaFileId);
        settings.SetFaviconMediaFileId(req.FaviconMediaFileId);
        settings.SetHeadingFont(req.HeadingFont);
        settings.SetBodyFont(req.BodyFont);

        await _settingsRepository.Update(settings);
        await Send.OkAsync(_mapper.Map<SiteSettingsDto>(settings), cancellation: ct);
    }
}
