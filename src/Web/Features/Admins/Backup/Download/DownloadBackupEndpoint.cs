using System.Net.Mime;
using Application.Interfaces.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Backup.Download;

public class DownloadBackupRequest
{
    public string FileName { get; set; } = null!;
}

public class DownloadBackupEndpoint : Endpoint<DownloadBackupRequest>
{
    private readonly IBackupService _backupService;

    public DownloadBackupEndpoint(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/backup/{fileName}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DownloadBackupRequest req, CancellationToken ct)
    {
        await using var stream = await _backupService.GetFileStreamAsync(req.FileName, ct);
        if (stream is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        HttpContext.Response.ContentType = "application/octet-stream";
        var disposition = new ContentDisposition { FileName = req.FileName, DispositionType = "attachment" };
        HttpContext.Response.Headers.Append("Content-Disposition", disposition.ToString());
        await stream.CopyToAsync(HttpContext.Response.Body, ct);
    }
}
