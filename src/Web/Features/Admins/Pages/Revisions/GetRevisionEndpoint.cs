using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Pages.Revisions;

public class GetRevisionRequest
{
    public Guid PageId { get; set; }
    public Guid RevisionId { get; set; }
}

public class GetRevisionEndpoint : Endpoint<GetRevisionRequest, PageRevisionDto>
{
    private readonly IPageRevisionRepository _revisionRepository;
    private readonly IMapper _mapper;

    public GetRevisionEndpoint(IPageRevisionRepository revisionRepository, IMapper mapper)
    {
        _revisionRepository = revisionRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/pages/{pageId}/revisions/{revisionId}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetRevisionRequest req, CancellationToken ct)
    {
        var revision = _revisionRepository.FindById(req.RevisionId);
        if (revision is null || revision.PageId != req.PageId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(_mapper.Map<PageRevisionDto>(revision), cancellation: ct);
    }
}
