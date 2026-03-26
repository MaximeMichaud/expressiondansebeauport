using Application.Interfaces.FileStorage;
using Application.Interfaces.Services.Users;
using Domain.Common;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Social.Upload;

public class UploadMediaEndpoint : EndpointWithoutRequest
{
    private readonly IFileStorageApiConsumer _fileStorage;
    private readonly IAuthenticatedUserService _authenticatedUserService;
    private readonly IMemberRepository _memberRepository;

    public UploadMediaEndpoint(
        IFileStorageApiConsumer fileStorage,
        IAuthenticatedUserService authenticatedUserService,
        IMemberRepository memberRepository)
    {
        _fileStorage = fileStorage;
        _authenticatedUserService = authenticatedUserService;
        _memberRepository = memberRepository;
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
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            await Send.OkAsync(new SucceededOrNotResponse(false, new Error("InvalidType", "File type not allowed.")), ct);
            return;
        }

        if (file.Length > 50 * 1024 * 1024) // 50 MB
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
    }
}
