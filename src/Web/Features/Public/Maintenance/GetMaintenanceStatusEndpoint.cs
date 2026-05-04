using Domain.Repositories;
using FastEndpoints;

namespace Web.Features.Public.Maintenance;

public class MaintenanceStatusResponse
{
    public bool IsMaintenanceMode { get; set; }
    public string MaintenanceMessage { get; set; } = null!;
    public int MaintenanceRetryAfter { get; set; }
}

public class GetMaintenanceStatusEndpoint : EndpointWithoutRequest<MaintenanceStatusResponse>
{
    private readonly ISiteSettingsRepository _settingsRepository;

    public GetMaintenanceStatusEndpoint(ISiteSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("public/maintenance-status");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var settings = await _settingsRepository.Get();
        await Send.OkAsync(new MaintenanceStatusResponse
        {
            IsMaintenanceMode = settings.IsMaintenanceMode,
            MaintenanceMessage = settings.MaintenanceMessage,
            MaintenanceRetryAfter = settings.MaintenanceRetryAfter
        }, cancellation: ct);
    }
}
