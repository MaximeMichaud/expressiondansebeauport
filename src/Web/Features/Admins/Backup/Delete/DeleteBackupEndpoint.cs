using Application.Interfaces.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Backup.Delete;

public class DeleteBackupRequest
{
    public Guid Id { get; set; }
}

public class DeleteBackupEndpoint : Endpoint<DeleteBackupRequest>
{
    private readonly IBackupService _backupService;

    public DeleteBackupEndpoint(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/backup/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteBackupRequest req, CancellationToken ct)
    {
        await _backupService.DeleteAsync(req.Id, ct);
        await Send.NoContentAsync(ct);
    }
}
