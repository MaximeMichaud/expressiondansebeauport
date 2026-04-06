using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Helpers;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.Pages.Revisions;

public class AutosaveRequest
{
    public Guid PageId { get; set; }
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public string? CustomCss { get; set; }
    public string ContentMode { get; set; } = "html";
    public string? Blocks { get; set; }
    public string? MetaDescription { get; set; }
    public string Status { get; set; } = "Draft";
}

public class AutosaveResponse
{
    public string SavedAt { get; set; } = null!;
}

public class AutosaveEndpoint : Endpoint<AutosaveRequest, AutosaveResponse>
{
    private readonly IPageRepository _pageRepository;
    private readonly IPageRevisionRepository _revisionRepository;
    private readonly IHttpContextUserService _userService;

    public AutosaveEndpoint(IPageRepository pageRepository, IPageRevisionRepository revisionRepository, IHttpContextUserService userService)
    {
        _pageRepository = pageRepository;
        _revisionRepository = revisionRepository;
        _userService = userService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/pages/{pageId}/autosave");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(AutosaveRequest req, CancellationToken ct)
    {
        var page = _pageRepository.FindById(req.PageId);
        if (page is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Supprimer l'autosave précédente (une seule par page)
        await _revisionRepository.DeleteAutosave(page.Id);

        var now = InstantHelper.GetLocalNow();
        var revisionNumber = _revisionRepository.GetNextRevisionNumber(page.Id);

        // Créer une page temporaire pour utiliser CreateFromPage
        var tempPage = new Page(req.Title, page.Slug);
        tempPage.SetId(page.Id);
        tempPage.SetContent(req.Content);
        tempPage.SetCustomCss(req.CustomCss);
        tempPage.SetContentMode(req.ContentMode);
        tempPage.SetBlocks(req.Blocks);
        tempPage.SetMetaDescription(req.MetaDescription);
        if (Enum.TryParse<PageStatus>(req.Status, true, out var status) && status == PageStatus.Published)
            tempPage.Publish();

        var revision = PageRevision.CreateFromPage(tempPage, revisionNumber, RevisionType.Autosave, _userService.Username, now);
        await _revisionRepository.Create(revision);

        await Send.OkAsync(new AutosaveResponse { SavedAt = now.ToString() }, cancellation: ct);
    }
}
