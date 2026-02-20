using Application.Interfaces.FileStorage;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Media.Upload;

public class UploadMediaRequest
{
    public IFormFile File { get; set; } = null!;
    public string? AltText { get; set; }
}

public class UploadMediaValidator : Validator<UploadMediaRequest>
{
    public UploadMediaValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithErrorCode("FileRequired")
            .WithMessage("A file is required.");
    }
}

public class UploadMediaEndpoint : Endpoint<UploadMediaRequest, MediaFileDto>
{
    private readonly IFileStorageApiConsumer _fileStorage;
    private readonly IMediaFileRepository _mediaFileRepository;
    private readonly IMapper _mapper;

    public UploadMediaEndpoint(IFileStorageApiConsumer fileStorage, IMediaFileRepository mediaFileRepository, IMapper mapper)
    {
        _fileStorage = fileStorage;
        _mediaFileRepository = mediaFileRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/media");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadMediaRequest req, CancellationToken ct)
    {
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(req.File.FileName)}";
        var blobUrl = await _fileStorage.UploadFileAsync(req.File);

        var mediaFile = new MediaFile(
            uniqueFileName,
            req.File.FileName,
            req.File.ContentType,
            req.File.Length,
            blobUrl);

        mediaFile.SetAltText(req.AltText);

        await _mediaFileRepository.Create(mediaFile);

        var response = _mapper.Map<MediaFileDto>(mediaFile);
        await Send.OkAsync(response, cancellation: ct);
    }
}
