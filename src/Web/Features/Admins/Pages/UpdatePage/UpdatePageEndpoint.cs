using Domain.Common;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Features.Common;

namespace Web.Features.Admins.Pages.UpdatePage;

public class UpdatePageEndpoint : EndpointWithSanitizedRequest<UpdatePageRequest, SucceededOrNotResponse>
{
    private readonly IPageRepository _pageRepository;

    public UpdatePageEndpoint(IPageRepository pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("admin/pages/{id}");

        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdatePageRequest req, CancellationToken ct)
    {
        var page = _pageRepository.FindById(req.Id);

        page.SetTitle(req.Title);

        var incomingIds = req.Sections.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToHashSet();
        var sectionsToRemove = page.Sections.Where(s => !incomingIds.Contains(s.Id)).ToList();
        foreach (var section in sectionsToRemove)
        {
            page.Sections.Remove(section);
        }

        foreach (var sectionReq in req.Sections)
        {
            if (sectionReq.Id.HasValue)
            {
                var existing = page.Sections.FirstOrDefault(s => s.Id == sectionReq.Id.Value);
                if (existing != null)
                {
                    existing.SetTitle(sectionReq.Title);
                    existing.SetHtmlContent(sectionReq.HtmlContent);
                    existing.SetImageUrl(sectionReq.ImageUrl);
                    existing.SetSortOrder(sectionReq.SortOrder);
                }
            }
            else
            {
                var newSection = new PageSection(sectionReq.Title, sectionReq.HtmlContent, sectionReq.ImageUrl, sectionReq.SortOrder);
                page.AddSection(newSection);
            }
        }

        await _pageRepository.Update(page);
        await Send.OkAsync(new SucceededOrNotResponse(true), cancellation: ct);
    }
}
