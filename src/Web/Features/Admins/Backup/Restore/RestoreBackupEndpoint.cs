using Application.Interfaces.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Backup.Restore;

public class RestoreBackupRequest
{
    public string FileName { get; set; } = null!;
}

public class RestoreBackupEndpoint : Endpoint<RestoreBackupRequest>
{
    private readonly IBackupService _backupService;

    public RestoreBackupEndpoint(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/backup/restore");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(RestoreBackupRequest req, CancellationToken ct)
    {
        await _backupService.RestoreAsync(req.FileName, ct);
        await Send.OkAsync(new { message = "Restauration terminée" }, cancellation: ct);
    }
}
