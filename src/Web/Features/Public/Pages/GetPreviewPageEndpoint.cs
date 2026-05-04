using Domain.Helpers;
using Domain.Repositories;
using FastEndpoints;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Public.Pages;

public class GetPreviewPageRequest
{
    public string Slug { get; set; } = null!;

    [QueryParam]
    public string Token { get; set; } = null!;
}

public class GetPreviewPageEndpoint : Endpoint<GetPreviewPageRequest, PageDto>
{
    private readonly IPageRepository _pageRepository;
    private readonly IPreviewTokenRepository _previewTokenRepository;
    private readonly IPageRevisionRepository _revisionRepository;
    private readonly IMapper _mapper;

    public GetPreviewPageEndpoint(IPageRepository pageRepository, IPreviewTokenRepository previewTokenRepository, IPageRevisionRepository revisionRepository, IMapper mapper)
    {
        _pageRepository = pageRepository;
        _previewTokenRepository = previewTokenRepository;
        _revisionRepository = revisionRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("public/pages/preview/{slug}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetPreviewPageRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Token))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var previewToken = await _previewTokenRepository.FindByToken(req.Token);
        if (previewToken is null || previewToken.IsExpired(InstantHelper.GetLocalNow()))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var page = previewToken.Page;
        if (page.Slug != req.Slug)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Chercher l'autosave la plus récente, sinon retourner la page telle quelle
        var autosave = await _revisionRepository.GetAutosave(page.Id);
        if (autosave is not null)
        {
            // Retourner le contenu de l'autosave dans le format PageDto
            var dto = _mapper.Map<PageDto>(page);
            dto.Title = autosave.Title;
            dto.Content = autosave.Content;
            dto.CustomCss = autosave.CustomCss;
            dto.ContentMode = autosave.ContentMode;
            dto.Blocks = autosave.Blocks;
            dto.MetaDescription = autosave.MetaDescription;
            await Send.OkAsync(dto, cancellation: ct);
            return;
        }

        await Send.OkAsync(_mapper.Map<PageDto>(page), cancellation: ct);
    }
}
