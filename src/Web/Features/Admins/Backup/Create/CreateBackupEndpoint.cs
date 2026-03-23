using Application.Interfaces.Services;
using Domain.Entities;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Backup.Create;

public class CreateBackupEndpoint : EndpointWithoutRequest
{
    private readonly IBackupService _backupService;

    public CreateBackupEndpoint(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/backup");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var record = await _backupService.CreateBackupAsync(BackupType.Manual, ct);
        await Send.OkAsync(new
        {
            record.Id,
            record.FileName,
            record.SizeInBytes,
            CreatedAt = record.CreatedAt.ToString(),
            record.Type,
            record.Status
        }, cancellation: ct);
    }
}
