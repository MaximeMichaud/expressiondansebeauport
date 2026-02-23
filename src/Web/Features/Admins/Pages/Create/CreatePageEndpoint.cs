using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.Pages.Create;

public class CreatePageRequest
{
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public string Status { get; set; } = "Draft";
    public Guid? FeaturedImageId { get; set; }
    public string? MetaDescription { get; set; }
    public int SortOrder { get; set; }
}

public class CreatePageValidator : Validator<CreatePageRequest>
{
    public CreatePageValidator()
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

public class CreatePageEndpoint : Endpoint<CreatePageRequest, PageDto>
{
    private readonly IPageRepository _pageRepository;
    private readonly IMapper _mapper;

    public CreatePageEndpoint(IPageRepository pageRepository, IMapper mapper)
    {
        _pageRepository = pageRepository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/pages");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreatePageRequest req, CancellationToken ct)
    {
        var slug = string.IsNullOrWhiteSpace(req.Slug) ? Page.GenerateSlug(req.Title) : Page.GenerateSlug(req.Slug);

        if (_pageRepository.SlugExists(slug))
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";

        var page = new Page(req.Title, slug);
        page.SetContent(req.Content);
        page.SetFeaturedImageId(req.FeaturedImageId);
        page.SetMetaDescription(req.MetaDescription);
        page.SetSortOrder(req.SortOrder);

        if (Enum.TryParse<PageStatus>(req.Status, true, out var status) && status == PageStatus.Published)
            page.Publish();

        await _pageRepository.Create(page);
        await Send.OkAsync(_mapper.Map<PageDto>(page), cancellation: ct);
    }
}
