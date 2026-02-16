using Domain.Repositories;
using FastEndpoints;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Public.Pages.GetPageBySlug;

public class GetPageBySlugEndpoint : Endpoint<GetPageBySlugRequest, PageDto>
{
    private readonly IMapper _mapper;
    private readonly IPageRepository _pageRepository;

    public GetPageBySlugEndpoint(IMapper mapper, IPageRepository pageRepository)
    {
        _mapper = mapper;
        _pageRepository = pageRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("pages/{slug}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetPageBySlugRequest req, CancellationToken ct)
    {
        var page = _pageRepository.FindBySlug(req.Slug);
        var response = _mapper.Map<PageDto>(page);
        await Send.OkAsync(response, cancellation: ct);
    }
}
