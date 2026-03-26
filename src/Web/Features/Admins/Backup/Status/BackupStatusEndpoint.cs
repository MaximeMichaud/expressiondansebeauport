using Application.Interfaces.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Backup.Status;

public class BackupStatusEndpoint : EndpointWithoutRequest
{
    private readonly IBackupService _backupService;

    public BackupStatusEndpoint(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public override void Configure()
    {
        Get("admin/backup/status");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(new { available = _backupService.IsAvailable() }, cancellation: ct);
    }
}
