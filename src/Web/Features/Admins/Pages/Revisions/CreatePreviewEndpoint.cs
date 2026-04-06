using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Helpers;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NodaTime;

namespace Web.Features.Admins.Pages.Revisions;

public class CreatePreviewRequest
{
    public Guid PageId { get; set; }
}

public class CreatePreviewResponse
{
    public string Token { get; set; } = null!;
    public string PreviewUrl { get; set; } = null!;
}

public class CreatePreviewEndpoint : Endpoint<CreatePreviewRequest, CreatePreviewResponse>
{
    private readonly IPageRepository _pageRepository;
    private readonly IPreviewTokenRepository _previewTokenRepository;
    private readonly IHttpContextUserService _userService;

    public CreatePreviewEndpoint(IPageRepository pageRepository, IPreviewTokenRepository previewTokenRepository, IHttpContextUserService userService)
    {
        _pageRepository = pageRepository;
        _previewTokenRepository = previewTokenRepository;
        _userService = userService;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/pages/{pageId}/preview");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreatePreviewRequest req, CancellationToken ct)
    {
        var page = _pageRepository.FindById(req.PageId);
        if (page is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Nettoyer les tokens expirés
        await _previewTokenRepository.DeleteExpired();

        var now = InstantHelper.GetLocalNow();
        var previewToken = new PreviewToken(page.Id, Duration.FromMinutes(30), _userService.Username, now);
        await _previewTokenRepository.Create(previewToken);

        await Send.OkAsync(new CreatePreviewResponse
        {
            Token = previewToken.Token,
            PreviewUrl = $"/preview/{page.Slug}?token={previewToken.Token}"
        }, cancellation: ct);
    }
}
