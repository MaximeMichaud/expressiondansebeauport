using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.HelpArticles.GetAll;

public class GetAllHelpArticlesRequest
{
    public string? Category { get; set; }
    public bool? IsPublished { get; set; }
}

public class GetAllHelpArticlesEndpoint : Endpoint<GetAllHelpArticlesRequest, List<HelpArticleDto>>
{
    private readonly IHelpArticleRepository _repository;
    private readonly IMapper _mapper;

    public GetAllHelpArticlesEndpoint(IHelpArticleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/help-articles");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetAllHelpArticlesRequest req, CancellationToken ct)
    {
        HelpCategory? categoryFilter = null;
        if (!string.IsNullOrWhiteSpace(req.Category)
            && Enum.TryParse<HelpCategory>(req.Category, true, out var parsed))
        {
            categoryFilter = parsed;
        }

        var articles = await _repository.GetAll(categoryFilter, req.IsPublished);
        var ordered = articles
            .OrderBy(a => a.Category)
            .ThenBy(a => a.SortOrder)
            .ToList();

        await Send.OkAsync(_mapper.Map<List<HelpArticleDto>>(ordered), cancellation: ct);
    }
}
