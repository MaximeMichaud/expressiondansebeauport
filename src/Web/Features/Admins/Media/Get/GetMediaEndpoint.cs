using Domain.Repositories;
using IMapper = AutoMapper.IMapper;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;

namespace Web.Features.Admins.Media.Get;

public class GetMediaRequest
{
    public Guid Id { get; set; }
}

public class GetMediaEndpoint : Endpoint<GetMediaRequest, MediaFileDto>
{
    private readonly IMediaFileRepository _mediaFileRepository;
    private readonly IMapper _mapper;

    public GetMediaEndpoint(IMediaFileRepository mediaFileRepository, IMapper mapper)
    {
        _mediaFileRepository = mediaFileRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/media/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetMediaRequest req, CancellationToken ct)
    {
        var mediaFile = _mediaFileRepository.FindById(req.Id);
        if (mediaFile is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = _mapper.Map<MediaFileDto>(mediaFile);
        await Send.OkAsync(response, cancellation: ct);
    }
}
