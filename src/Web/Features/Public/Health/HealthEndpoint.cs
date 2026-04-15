using FastEndpoints;

namespace Web.Features.Public.Health;

/// <summary>
/// Endpoint de santé public pour les health checks Docker et Caddy.
/// Contrairement à l'endpoint admin, celui-ci ne requiert aucune authentification.
/// </summary>
public class HealthEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("health");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken ct)
        => Send.OkAsync(new { status = "healthy" }, cancellation: ct);
}
