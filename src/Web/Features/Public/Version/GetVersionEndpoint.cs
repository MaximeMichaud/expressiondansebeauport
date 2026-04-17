using System.Reflection;
using FastEndpoints;

namespace Web.Features.Public.Version;

public class VersionResponse
{
    public string Version { get; set; } = null!;
    public long BuiltAt { get; set; }
}

public class GetVersionEndpoint : EndpointWithoutRequest<VersionResponse>
{
    private static readonly string _version = ResolveVersion();
    private static readonly long _builtAt = ResolveBuiltAt();

    public override void Configure()
    {
        DontCatchExceptions();
        Get("public/version");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        HttpContext.Response.Headers.CacheControl = "no-store";
        await Send.OkAsync(new VersionResponse { Version = _version, BuiltAt = _builtAt }, cancellation: ct);
    }

    private static string ResolveVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        return informational ?? assembly.GetName().Version?.ToString() ?? "0.0.0";
    }

    private static long ResolveBuiltAt()
    {
        var location = Assembly.GetExecutingAssembly().Location;
        if (string.IsNullOrEmpty(location) || !File.Exists(location))
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        return new DateTimeOffset(File.GetLastWriteTimeUtc(location)).ToUnixTimeSeconds();
    }
}
