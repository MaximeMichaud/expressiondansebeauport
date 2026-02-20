using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Pages.Update;

public class UpdatePageRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? FeaturedImageId { get; set; }
    public string? MetaDescription { get; set; }
    public int SortOrder { get; set; }
}

public class UpdatePageValidator : Validator<UpdatePageRequest>
{
    public UpdatePageValidator()
    {
        RuleFor(x => x.Title)
            .NotNull().NotEmpty()
            .WithErrorCode("TitleRequired")
            .WithMessage("Title is required.")
            .MaximumLength(200)
            .WithErrorCode("TitleTooLong")
            .WithMessage("Title must be 200 characters or less.");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(320)
            .WithErrorCode("MetaDescriptionTooLong")
            .WithMessage("Meta description must be 320 characters or less.");
    }
}

public class UpdatePageEndpoint : Endpoint<UpdatePageRequest, PageDto>
{
    private readonly IPageRepository _pageRepository;
    private readonly IMapper _mapper;

    public UpdatePageEndpoint(IPageRepository pageRepository, IMapper mapper)
    {
        _pageRepository = pageRepository;
        _mapper = mapper;
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
        if (page is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        page.SetTitle(req.Title);
        if (!string.IsNullOrWhiteSpace(req.Slug))
        {
            var slug = Page.GenerateSlug(req.Slug);
            if (_pageRepository.SlugExists(slug, req.Id))
                slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
            page.SetSlug(slug);
        }
        page.SetContent(req.Content);
        page.SetFeaturedImageId(req.FeaturedImageId);
        page.SetMetaDescription(req.MetaDescription);
        page.SetSortOrder(req.SortOrder);

        if (Enum.TryParse<PageStatus>(req.Status, true, out var status))
        {
            if (status == PageStatus.Published) page.Publish();
            else page.SetToDraft();
        }

        await _pageRepository.Update(page);
        await Send.OkAsync(_mapper.Map<PageDto>(page), cancellation: ct);
    }
}
