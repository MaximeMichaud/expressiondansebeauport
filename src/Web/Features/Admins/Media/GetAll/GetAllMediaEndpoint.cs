using Domain.Common;
using Domain.Entities;
using IMapper = AutoMapper.IMapper;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using Web.Features.Common;

namespace Web.Features.Admins.Media.GetAll;

public class GetAllMediaRequest : PaginateRequest
{
    public string? FileType { get; set; }
}

public class GetAllMediaEndpoint : Endpoint<GetAllMediaRequest, PaginatedList<MediaFileDto>>
{
    private readonly IMediaFileRepository _mediaFileRepository;
    private readonly IMapper _mapper;

    public GetAllMediaEndpoint(IMediaFileRepository mediaFileRepository, IMapper mapper)
    {
        _mediaFileRepository = mediaFileRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/media");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAllMediaRequest req, CancellationToken ct)
    {
        MediaFileType? fileType = null;
        if (Enum.TryParse<MediaFileType>(req.FileType, true, out var parsed))
            fileType = parsed;

        var paginatedList = _mediaFileRepository.GetAllPaginated(req.NormalizedPageIndex, req.NormalizedPageSize, fileType);
        var dtos = _mapper.Map<List<MediaFileDto>>(paginatedList.Items);
        await Send.OkAsync(new PaginatedList<MediaFileDto>(dtos, paginatedList.TotalItems), cancellation: ct);
    }
}
