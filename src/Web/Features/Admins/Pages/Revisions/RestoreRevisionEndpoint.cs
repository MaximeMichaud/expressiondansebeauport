using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Helpers;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Pages.Revisions;

public class RestoreRevisionRequest
{
    public Guid PageId { get; set; }
    public Guid RevisionId { get; set; }
}

public class RestoreRevisionEndpoint : Endpoint<RestoreRevisionRequest, PageDto>
{
    private readonly IPageRepository _pageRepository;
    private readonly IPageRevisionRepository _revisionRepository;
    private readonly IHttpContextUserService _userService;
    private readonly IMapper _mapper;

    public RestoreRevisionEndpoint(IPageRepository pageRepository, IPageRevisionRepository revisionRepository, IHttpContextUserService userService, IMapper mapper)
    {
        _pageRepository = pageRepository;
        _revisionRepository = revisionRepository;
        _userService = userService;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/pages/{pageId}/revisions/{revisionId}/restore");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(RestoreRevisionRequest req, CancellationToken ct)
    {
        var page = _pageRepository.FindById(req.PageId);
        if (page is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var revision = _revisionRepository.FindById(req.RevisionId);
        if (revision is null || revision.PageId != req.PageId)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Créer une révision de l'état actuel avant la restauration
        var revisionNumber = _revisionRepository.GetNextRevisionNumber(page.Id);
        var snapshotBeforeRestore = PageRevision.CreateFromPage(page, revisionNumber, RevisionType.Manual, _userService.Username, InstantHelper.GetLocalNow());
        await _revisionRepository.Create(snapshotBeforeRestore);

        // Appliquer le contenu de la révision sur la page
        page.SetTitle(revision.Title);
        page.SetContent(revision.Content);
        page.SetCustomCss(revision.CustomCss);
        page.SetContentMode(revision.ContentMode);
        page.SetBlocks(revision.Blocks);
        page.SetMetaDescription(revision.MetaDescription);
        if (revision.Status == PageStatus.Published) page.Publish();
        else page.SetToDraft();

        await _pageRepository.Update(page);
        await _revisionRepository.DeleteOldRevisions(page.Id, 25);
        await _revisionRepository.DeleteAutosave(page.Id);

        await Send.OkAsync(_mapper.Map<PageDto>(page), cancellation: ct);
    }
}
