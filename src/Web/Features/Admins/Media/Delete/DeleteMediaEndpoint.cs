using Application.Interfaces.FileStorage;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Web.Features.Admins.Media.Delete;

public class DeleteMediaRequest
{
    public Guid Id { get; set; }
}

public class DeleteMediaEndpoint : Endpoint<DeleteMediaRequest, EmptyResponse>
{
    private readonly IMediaFileRepository _mediaFileRepository;
    private readonly IFileStorageApiConsumer _fileStorage;
    private readonly GarneauTemplateDbContext _context;

    public DeleteMediaEndpoint(IMediaFileRepository mediaFileRepository, IFileStorageApiConsumer fileStorage, GarneauTemplateDbContext context)
    {
        _mediaFileRepository = mediaFileRepository;
        _fileStorage = fileStorage;
        _context = context;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/media/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteMediaRequest req, CancellationToken ct)
    {
        var mediaFile = _mediaFileRepository.FindById(req.Id);
        if (mediaFile is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (await IsProtectedMedia(req.Id, ct))
        {
            await Send.StringAsync(string.Empty, StatusCodes.Status409Conflict, cancellation: ct);
            return;
        }

        await _mediaFileRepository.Delete(mediaFile);
        await _fileStorage.DeleteFileWithUrl(mediaFile.BlobUrl);
        await Send.NoContentAsync(ct);
    }

    private async Task<bool> IsProtectedMedia(Guid mediaFileId, CancellationToken ct)
    {
        return await _context.SiteSettings.AnyAsync(s =>
                s.LogoMediaFileId == mediaFileId || s.FaviconMediaFileId == mediaFileId, ct)
            || await _context.FooterPartners.AnyAsync(p => p.MediaFileId == mediaFileId, ct);
    }
}
