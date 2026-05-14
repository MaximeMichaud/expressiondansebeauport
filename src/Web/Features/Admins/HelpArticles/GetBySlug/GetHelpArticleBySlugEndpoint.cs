using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.HelpArticles.GetBySlug;

public class GetHelpArticleBySlugRequest
{
    public string Slug { get; set; } = null!;
}

public class GetHelpArticleBySlugEndpoint : Endpoint<GetHelpArticleBySlugRequest, HelpArticleDto>
{
    private readonly IHelpArticleRepository _repository;
    private readonly IMapper _mapper;

    public GetHelpArticleBySlugEndpoint(IHelpArticleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/help-articles/by-slug/{slug}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetHelpArticleBySlugRequest req, CancellationToken ct)
    {
        var article = await _repository.GetBySlug(req.Slug);
        if (article is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(_mapper.Map<HelpArticleDto>(article), cancellation: ct);
    }
}
