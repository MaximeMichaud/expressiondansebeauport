using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using Web.Services;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.HelpArticles.Create;

public class CreateHelpArticleRequest
{
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public string Category { get; set; } = null!;
    public string? Content { get; set; }
    public string ContentMode { get; set; } = "html";
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
    public string? RouteHint { get; set; }
}

public class CreateHelpArticleValidator : Validator<CreateHelpArticleRequest>
{
    public CreateHelpArticleValidator()
    {
        RuleFor(x => x.Title)
            .NotNull().NotEmpty()
            .WithErrorCode("TitleRequired")
            .WithMessage("Title is required.")
            .MaximumLength(200)
            .WithErrorCode("TitleTooLong")
            .WithMessage("Title must be 200 characters or less.");

        RuleFor(x => x.Category)
            .NotNull().NotEmpty()
            .WithErrorCode("CategoryRequired")
            .WithMessage("Category is required.")
            .Must(value => Enum.TryParse<HelpCategory>(value, true, out _))
            .WithErrorCode("InvalidCategory")
            .WithMessage("Category must be a valid HelpCategory value.");

        RuleFor(x => x.RouteHint)
            .MaximumLength(200)
            .WithErrorCode("RouteHintTooLong")
            .WithMessage("Route hint must be 200 characters or less.");

        RuleFor(x => x.ContentMode)
            .Must(x => x is "html" or "blocks")
            .WithErrorCode("InvalidContentMode")
            .WithMessage("Content mode must be 'html' or 'blocks'.");
    }
}

public class CreateHelpArticleEndpoint : Endpoint<CreateHelpArticleRequest, HelpArticleDto>
{
    private readonly IHelpArticleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IHelpHtmlSanitizer _sanitizer;

    public CreateHelpArticleEndpoint(IHelpArticleRepository repository, IMapper mapper, IConfiguration configuration, IHelpHtmlSanitizer sanitizer)
    {
        _repository = repository;
        _mapper = mapper;
        _configuration = configuration;
        _sanitizer = sanitizer;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Post("admin/help-articles");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateHelpArticleRequest req, CancellationToken ct)
    {
        if (!_configuration.GetValue<bool>("HelpArticles:EditingEnabled"))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }
        var category = Enum.Parse<HelpCategory>(req.Category, true);
        var slug = string.IsNullOrWhiteSpace(req.Slug)
            ? HelpArticle.GenerateSlug(req.Title)
            : HelpArticle.GenerateSlug(req.Slug);

        if (await _repository.SlugExists(slug))
        {
            AddError(r => r.Slug, "Un article avec ce slug existe déjà.", "DuplicateSlug");
            await Send.ErrorsAsync(409, ct);
            return;
        }

        var article = new HelpArticle(req.Title, slug, category);
        article.SetContent(_sanitizer.Sanitize(req.Content));
        article.SetContentMode(req.ContentMode);
        article.SetSortOrder(req.SortOrder);
        article.SetRouteHint(req.RouteHint);

        if (req.IsPublished)
            article.Publish();
        else
            article.Unpublish();

        await _repository.Create(article);
        await Send.OkAsync(_mapper.Map<HelpArticleDto>(article), cancellation: ct);
    }
}
