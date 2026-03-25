using Application.Interfaces.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Backup.List;

public class BackupRecordDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public long SizeInBytes { get; set; }
    public string CreatedAt { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? ErrorMessage { get; set; }
}

public class ListBackupsEndpoint : EndpointWithoutRequest<List<BackupRecordDto>>
{
    private readonly IBackupService _backupService;

    public ListBackupsEndpoint(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/backups");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var records = _backupService.GetAll();
        var dtos = records.Select(r => new BackupRecordDto
        {
            Id = r.Id,
            FileName = r.FileName,
            SizeInBytes = r.SizeInBytes,
            CreatedAt = r.CreatedAt.ToString(),
            Type = r.Type,
            Status = r.Status,
            ErrorMessage = r.ErrorMessage
        }).ToList();

        await Send.OkAsync(dtos, cancellation: ct);
    }
}
