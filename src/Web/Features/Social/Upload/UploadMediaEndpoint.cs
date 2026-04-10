using Application.Interfaces.FileStorage;
using Application.Interfaces.Imaging;
using Application.Interfaces.Services.Users;
using Domain.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Upload;

public class UploadMediaEndpoint : EndpointWithoutRequest
{
    private const string SocialSubDirectory = "social";
    private const long MaxImageSize = 10 * 1024 * 1024; // 10 MB
    private const long MaxPdfSize = 50 * 1024 * 1024; // 50 MB

    private static readonly string[] AllowedImageTypes =
    {
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"
    };

    private readonly IFileStorageApiConsumer _fileStorage;
    private readonly IImageProcessor _imageProcessor;
    private readonly IAuthenticatedUserService _authenticatedUserService;

    public UploadMediaEndpoint(
        IFileStorageApiConsumer fileStorage,
        IImageProcessor imageProcessor,
        IAuthenticatedUserService authenticatedUserService)
    {
        _fileStorage = fileStorage;
        _imageProcessor = imageProcessor;
        _authenticatedUserService = authenticatedUserService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("social/upload");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = _authenticatedUserService.GetAuthenticatedUser();
        if (user == null) { await Send.UnauthorizedAsync(ct); return; }

        if (Files.Count == 0)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("NoFile", "No file uploaded.")), ct);
            return;
        }

        var file = Files[0];
        var contentType = file.ContentType.ToLowerInvariant();

        if (contentType == "application/pdf")
        {
            if (file.Length > MaxPdfSize)
            {
                await Send.OkAsync(new SucceededOrNotResponse(false, new Error("TooLarge", "File too large. Max 50MB.")), ct);
                return;
            }

            var url = await _fileStorage.UploadFileAsync(file);
            await Send.OkAsync(new
            {
                Succeeded = true,
                Url = url,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length
            }, ct);
            return;
        }

        if (!AllowedImageTypes.Contains(contentType))
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("InvalidType", "File type not allowed.")), ct);
            return;
        }

        if (file.Length > MaxImageSize)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("TooLarge", "Image too large. Max 10MB.")), ct);
            return;
        }

        ProcessedImage processed;
        try
        {
            processed = await _imageProcessor.ProcessImageAsync(file, ct);
        }
        catch (InvalidImageException)
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("InvalidImage", "Image is invalid or corrupted.")), ct);
            return;
        }

        try
        {
            var ticks = DateTime.Now.Ticks;
            var baseName = Path.GetFileNameWithoutExtension(file.FileName);
            var safeBase = string.Concat(baseName.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
            if (string.IsNullOrEmpty(safeBase)) safeBase = "img";

            var originalUrl = await _fileStorage.UploadStreamAsync(
                processed.OriginalStream,
                $"{safeBase}-{ticks}.original.{processed.OriginalFileExtension}",
                processed.OriginalContentType,
                SocialSubDirectory);

            var displayUrl = await _fileStorage.UploadStreamAsync(
                processed.DisplayStream,
                $"{safeBase}-{ticks}.display.webp",
                "image/webp",
                SocialSubDirectory);

            var thumbnailUrl = await _fileStorage.UploadStreamAsync(
                processed.ThumbnailStream,
                $"{safeBase}-{ticks}.thumb.webp",
                "image/webp",
                SocialSubDirectory);

            await Send.OkAsync(new
            {
                Succeeded = true,
                OriginalUrl = originalUrl,
                DisplayUrl = displayUrl,
                ThumbnailUrl = thumbnailUrl,
                ContentType = processed.OriginalContentType,
                Size = file.Length,
                Width = processed.Width,
                Height = processed.Height
            }, ct);
        }
        finally
        {
            processed.Dispose();
        }
    }
}
