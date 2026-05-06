using System.Text.Json;
using System.Text.Json.Nodes;
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
        dto.Blocks = StripPrivateBlockData(dto.Blocks);
        dto.Breadcrumbs = _breadcrumbService.GetForPage(page).ToList();

        await Send.OkAsync(dto, cancellation: ct);
    }

    private static string? StripPrivateBlockData(string? blocksJson)
    {
        if (string.IsNullOrWhiteSpace(blocksJson)) return blocksJson;

        try
        {
            if (JsonNode.Parse(blocksJson) is not JsonArray array) return blocksJson;

            foreach (var node in array)
            {
                if (node is not JsonObject block) continue;
                if (block["type"]?.GetValue<string>() != "contact-form") continue;
                if (block["data"] is not JsonObject data) continue;

                data.Remove("recipientEmail");
            }

            return array.ToJsonString();
        }
        catch (JsonException)
        {
            return blocksJson;
        }
    }
}
