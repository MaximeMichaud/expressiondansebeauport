using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using Web.Services;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.HelpArticles.Update;

public class UpdateHelpArticleRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Slug { get; set; }
    public string Category { get; set; } = null!;
    public string? Content { get; set; }
    public string ContentMode { get; set; } = "html";
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
    public string? RouteHint { get; set; }
}

public class UpdateHelpArticleValidator : Validator<UpdateHelpArticleRequest>
{
    public UpdateHelpArticleValidator()
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

public class UpdateHelpArticleEndpoint : Endpoint<UpdateHelpArticleRequest, HelpArticleDto>
{
    private readonly IHelpArticleRepository _repository;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IHelpHtmlSanitizer _sanitizer;

    public UpdateHelpArticleEndpoint(IHelpArticleRepository repository, IMapper mapper, IConfiguration configuration, IHelpHtmlSanitizer sanitizer)
    {
        _repository = repository;
        _mapper = mapper;
        _configuration = configuration;
        _sanitizer = sanitizer;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Put("admin/help-articles/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(UpdateHelpArticleRequest req, CancellationToken ct)
    {
        if (!_configuration.GetValue<bool>("HelpArticles:EditingEnabled"))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }
        var article = await _repository.GetById(req.Id);
        if (article is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var category = Enum.Parse<HelpCategory>(req.Category, true);

        article.SetTitle(req.Title);
        if (!string.IsNullOrWhiteSpace(req.Slug))
        {
            var slug = HelpArticle.GenerateSlug(req.Slug);
            if (string.IsNullOrEmpty(slug))
            {
                AddError(r => r.Slug, "Le slug normalisé est vide. Utilisez un slug contenant des caractères alphanumériques.", "InvalidSlug");
                await Send.ErrorsAsync(400, ct);
                return;
            }
            if (await _repository.SlugExists(slug, excludeId: article.Id))
            {
                AddError(r => r.Slug, "Un article avec ce slug existe déjà.", "DuplicateSlug");
                await Send.ErrorsAsync(409, ct);
                return;
            }
            article.SetSlug(req.Slug);
        }
        article.SetCategory(category);
        article.SetContent(_sanitizer.Sanitize(req.Content));
        article.SetContentMode(req.ContentMode);
        article.SetSortOrder(req.SortOrder);
        article.SetRouteHint(req.RouteHint);

        if (req.IsPublished)
            article.Publish();
        else
            article.Unpublish();

        await _repository.Update(article);
        await Send.OkAsync(_mapper.Map<HelpArticleDto>(article), cancellation: ct);
    }
}
