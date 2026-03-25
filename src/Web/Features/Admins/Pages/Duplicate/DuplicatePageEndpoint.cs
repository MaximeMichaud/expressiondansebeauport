using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Pages.Duplicate;

public class DuplicatePageRequest
{
    public Guid Id { get; set; }
}

public class DuplicatePageEndpoint : Endpoint<DuplicatePageRequest, PageDto>
{
    private readonly IPageRepository _pageRepository;
    private readonly IMapper _mapper;

    public DuplicatePageEndpoint(IPageRepository pageRepository, IMapper mapper)
    {
        _pageRepository = pageRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/pages/{id}/duplicate");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DuplicatePageRequest req, CancellationToken ct)
    {
        var source = _pageRepository.FindById(req.Id);
        if (source is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var slug = GenerateUniqueSlug(source.Slug);

        var duplicate = new Page(source.Title, slug);
        duplicate.SetContent(source.Content);
        duplicate.SetCustomCss(source.CustomCss);
        duplicate.SetFeaturedImageId(source.FeaturedImageId);
        duplicate.SetMetaDescription(source.MetaDescription);
        duplicate.SetSortOrder(source.SortOrder);

        await _pageRepository.Create(duplicate);
        await Send.OkAsync(_mapper.Map<PageDto>(duplicate), cancellation: ct);
    }

    private string GenerateUniqueSlug(string sourceSlug)
    {
        var candidate = $"{sourceSlug}-2";
        var counter = 2;

        while (_pageRepository.SlugExistsIncludingDeleted(candidate))
        {
            counter++;
            candidate = $"{sourceSlug}-{counter}";
        }

        return candidate;
    }
}
