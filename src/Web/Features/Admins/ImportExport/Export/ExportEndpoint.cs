using System.Text.Json;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.ImportExport.Export;

public class ExportEndpoint : EndpointWithoutRequest
{
    private readonly IPageRepository _pageRepository;
    private readonly IMediaFileRepository _mediaFileRepository;
    private readonly INavigationMenuRepository _menuRepository;
    private readonly ISiteSettingsRepository _settingsRepository;
    private readonly IMapper _mapper;

    public ExportEndpoint(
        IPageRepository pageRepository,
        IMediaFileRepository mediaFileRepository,
        INavigationMenuRepository menuRepository,
        ISiteSettingsRepository settingsRepository,
        IMapper mapper)
    {
        _pageRepository = pageRepository;
        _mediaFileRepository = mediaFileRepository;
        _menuRepository = menuRepository;
        _settingsRepository = settingsRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/export");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var allMedia = _mediaFileRepository.GetAllPaginated(1, 10000);
        var allPages = _pageRepository.GetAllPaginated(1, 10000);
        var allMenus = _menuRepository.GetAll();
        var settings = await _settingsRepository.Get();

        var export = new
        {
            ExportedAt = DateTime.UtcNow,
            Pages = _mapper.Map<List<PageDto>>(allPages.Items),
            MediaFiles = _mapper.Map<List<MediaFileDto>>(allMedia.Items),
            Menus = _mapper.Map<List<NavigationMenuDto>>(allMenus),
            SiteSettings = _mapper.Map<SiteSettingsDto>(settings)
        };

        var json = JsonSerializer.SerializeToUtf8Bytes(export, new JsonSerializerOptions { WriteIndented = true });
        var fileName = $"site-export-{DateTime.UtcNow:yyyy-MM-dd}.json";

        HttpContext.Response.ContentType = "application/json";
        HttpContext.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
        await HttpContext.Response.Body.WriteAsync(json, ct);
    }
}
