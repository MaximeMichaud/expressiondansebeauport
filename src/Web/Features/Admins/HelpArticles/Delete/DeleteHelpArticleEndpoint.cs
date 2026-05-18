using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.HelpArticles.Delete;

public class DeleteHelpArticleRequest
{
    public Guid Id { get; set; }
}

public class DeleteHelpArticleEndpoint : Endpoint<DeleteHelpArticleRequest, EmptyResponse>
{
    private readonly IHelpArticleRepository _repository;
    private readonly IConfiguration _configuration;

    public DeleteHelpArticleEndpoint(IHelpArticleRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Delete("admin/help-articles/{id}");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(DeleteHelpArticleRequest req, CancellationToken ct)
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
        await _repository.Delete(req.Id);
        await Send.NoContentAsync(ct);
    }
}
