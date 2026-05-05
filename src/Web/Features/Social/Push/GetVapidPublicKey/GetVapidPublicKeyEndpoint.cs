using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;

namespace Web.Features.Social.Push.GetVapidPublicKey;

public record GetVapidPublicKeyResponse(string PublicKey);

public class GetVapidPublicKeyEndpoint : EndpointWithoutRequest<GetVapidPublicKeyResponse>
{
    private readonly IConfiguration _config;

    public GetVapidPublicKeyEndpoint(IConfiguration config) => _config = config;

    public override void Configure()
    {
        Get("social/push/vapid-public-key");
        Roles(Domain.Constants.User.Roles.MEMBER, Domain.Constants.User.Roles.PROFESSOR, Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        var key = _config["Vapid:PublicKey"] ?? string.Empty;
        return Send.OkAsync(new GetVapidPublicKeyResponse(key), ct);
    }
}
