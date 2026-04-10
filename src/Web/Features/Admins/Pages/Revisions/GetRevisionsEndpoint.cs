using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Pages.Revisions;

public class GetRevisionsRequest
{
    public Guid PageId { get; set; }
}

public class GetRevisionsEndpoint : Endpoint<GetRevisionsRequest, List<PageRevisionListItemDto>>
{
    private readonly IPageRevisionRepository _revisionRepository;
    private readonly IMapper _mapper;

    public GetRevisionsEndpoint(IPageRevisionRepository revisionRepository, IMapper mapper)
    {
        _revisionRepository = revisionRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/pages/{pageId}/revisions");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetRevisionsRequest req, CancellationToken ct)
    {
        var revisions = _revisionRepository.GetByPageId(req.PageId);
        await Send.OkAsync(_mapper.Map<List<PageRevisionListItemDto>>(revisions), cancellation: ct);
    }
}
