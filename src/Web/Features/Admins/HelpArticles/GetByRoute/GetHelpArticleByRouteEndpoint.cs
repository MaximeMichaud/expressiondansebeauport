using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.HelpArticles.GetByRoute;

public class GetHelpArticleByRouteRequest
{
    public string RouteName { get; set; } = null!;
}

public class GetHelpArticleByRouteEndpoint : Endpoint<GetHelpArticleByRouteRequest, HelpArticleDto>
{
    private readonly IHelpArticleRepository _repository;
    private readonly IMapper _mapper;

    public GetHelpArticleByRouteEndpoint(IHelpArticleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/help-articles/by-route");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetHelpArticleByRouteRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RouteName))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var article = await _repository.GetByRouteHint(req.RouteName);
        if (article is null || !article.IsPublished)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(_mapper.Map<HelpArticleDto>(article), cancellation: ct);
    }
}
