using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Media.Update;

public class UpdateMediaRequest
{
    public Guid Id { get; set; }
    public string? AltText { get; set; }
}

public class UpdateMediaValidator : Validator<UpdateMediaRequest>
{
    public UpdateMediaValidator()
    {
        RuleFor(x => x.AltText)
            .MaximumLength(200)
            .WithErrorCode("AltTextTooLong")
            .WithMessage("Alt text must be 200 characters or less.");
    }
}

public class UpdateMediaEndpoint : Endpoint<UpdateMediaRequest, MediaFileDto>
{
    private readonly IMediaFileRepository _mediaFileRepository;
    private readonly IMapper _mapper;

    public UpdateMediaEndpoint(IMediaFileRepository mediaFileRepository, IMapper mapper)
    {
        _mediaFileRepository = mediaFileRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Patch("admin/media/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateMediaRequest req, CancellationToken ct)
    {
        var mediaFile = _mediaFileRepository.FindById(req.Id);
        if (mediaFile is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        mediaFile.SetAltText(req.AltText);
        await _mediaFileRepository.Update(mediaFile);

        var response = _mapper.Map<MediaFileDto>(mediaFile);
        await Send.OkAsync(response, cancellation: ct);
    }
}
