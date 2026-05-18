using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Web.Features.Admins.HelpArticles.Permissions;

public class HelpArticlesPermissionsDto
{
    public bool CanEdit { get; set; }
}

public class GetHelpArticlesPermissionsEndpoint : EndpointWithoutRequest<HelpArticlesPermissionsDto>
{
    private readonly IConfiguration _configuration;

    public GetHelpArticlesPermissionsEndpoint(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/help-articles/permissions");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var canEdit = _configuration.GetValue<bool>("HelpArticles:EditingEnabled");
        await Send.OkAsync(new HelpArticlesPermissionsDto { CanEdit = canEdit }, cancellation: ct);
    }
}
