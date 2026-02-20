using Application.Interfaces.FileStorage;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Media.Delete;

public class DeleteMediaRequest
{
    public Guid Id { get; set; }
}

public class DeleteMediaEndpoint : Endpoint<DeleteMediaRequest, EmptyResponse>
{
    private readonly IMediaFileRepository _mediaFileRepository;
    private readonly IFileStorageApiConsumer _fileStorage;

    public DeleteMediaEndpoint(IMediaFileRepository mediaFileRepository, IFileStorageApiConsumer fileStorage)
    {
        _mediaFileRepository = mediaFileRepository;
        _fileStorage = fileStorage;
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

        await _fileStorage.DeleteFileWithUrl(mediaFile.BlobUrl);
        await _mediaFileRepository.Delete(mediaFile);
        await Send.NoContentAsync(ct);
    }
}
