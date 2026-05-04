using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.SiteSettings.Maintenance;

public class ToggleMaintenanceModeRequest
{
    public bool IsMaintenanceMode { get; set; }
    public string MaintenanceMessage { get; set; } = "Le site est en maintenance. Revenez bientôt !";
    public int MaintenanceRetryAfter { get; set; } = 3600;
}

public class ToggleMaintenanceModeEndpoint : Endpoint<ToggleMaintenanceModeRequest, SiteSettingsDto>
{
    private readonly ISiteSettingsRepository _settingsRepository;
    private readonly IMapper _mapper;

    public ToggleMaintenanceModeEndpoint(ISiteSettingsRepository settingsRepository, IMapper mapper)
    {
        _settingsRepository = settingsRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/maintenance/toggle");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(ToggleMaintenanceModeRequest req, CancellationToken ct)
    {
        var settings = await _settingsRepository.Get();
        settings.SetMaintenanceMode(req.IsMaintenanceMode);
        settings.SetMaintenanceMessage(req.MaintenanceMessage);
        settings.SetMaintenanceRetryAfter(req.MaintenanceRetryAfter);
        await _settingsRepository.Update(settings);
        await Send.OkAsync(_mapper.Map<SiteSettingsDto>(settings), cancellation: ct);
    }
}
