using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Web.Dtos;
using Web.Features.Public.Breadcrumbs;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Public.Pages;

public class GetPublicPageRequest
{
    public string Slug { get; set; } = null!;
}

public class GetPublicPageEndpoint : Endpoint<GetPublicPageRequest, PageDto>
{
    private readonly IPageRepository _pageRepository;
    private readonly IBreadcrumbService _breadcrumbService;
    private readonly IMapper _mapper;

    public GetPublicPageEndpoint(
        IPageRepository pageRepository,
        IBreadcrumbService breadcrumbService,
        IMapper mapper)
    {
        _pageRepository = pageRepository;
        _breadcrumbService = breadcrumbService;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("public/pages/{slug}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetPublicPageRequest req, CancellationToken ct)
    {
        var page = _pageRepository.FindBySlug(req.Slug);
        if (page is null || page.Status != PageStatus.Published)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var dto = _mapper.Map<PageDto>(page);
        dto.Breadcrumbs = _breadcrumbService.GetForPage(page).ToList();

        await Send.OkAsync(dto, cancellation: ct);
    }
}
