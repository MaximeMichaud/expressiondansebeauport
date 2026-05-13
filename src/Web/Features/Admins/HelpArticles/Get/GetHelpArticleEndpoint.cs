using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Web.Dtos;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Admins.HelpArticles.Get;

public class GetHelpArticleRequest
{
    public Guid Id { get; set; }
}

public class GetHelpArticleEndpoint : Endpoint<GetHelpArticleRequest, HelpArticleDto>
{
    private readonly IHelpArticleRepository _repository;
    private readonly IMapper _mapper;

    public GetHelpArticleEndpoint(IHelpArticleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/help-articles/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetHelpArticleRequest req, CancellationToken ct)
    {
        var article = await _repository.GetById(req.Id);
        if (article is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(_mapper.Map<HelpArticleDto>(article), cancellation: ct);
    }
}
