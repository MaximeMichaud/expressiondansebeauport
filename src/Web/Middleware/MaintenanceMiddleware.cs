using Domain.Repositories;

namespace Web.Middleware;

public class MaintenanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MaintenanceMiddleware> _logger;

    public MaintenanceMiddleware(RequestDelegate next, ILogger<MaintenanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISiteSettingsRepository settingsRepository)
    {
        var envFlag = Environment.GetEnvironmentVariable("MAINTENANCE_MODE");
        var envMaintenanceActive = string.Equals(envFlag, "true", StringComparison.OrdinalIgnoreCase);

        var path = context.Request.Path.Value ?? "";

        var isAdminRoute = path.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/admins", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/authentication", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/public/maintenance-status", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase);

        if (isAdminRoute)
        {
            await _next(context);
            return;
        }

        bool dbMaintenanceActive = false;
        string message = "Le site est en maintenance. Revenez bientôt !";
        int retryAfter = 3600;

        if (!envMaintenanceActive)
        {
            try
            {
                var settings = await settingsRepository.Get();
                dbMaintenanceActive = settings.IsMaintenanceMode;
                message = settings.MaintenanceMessage;
                retryAfter = settings.MaintenanceRetryAfter;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read maintenance settings from database.");
            }
        }

        if (!envMaintenanceActive && !dbMaintenanceActive)
        {
            await _next(context);
            return;
        }

        if (envMaintenanceActive && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAINTENANCE_MESSAGE")))
            message = Environment.GetEnvironmentVariable("MAINTENANCE_MESSAGE")!;

        _logger.LogInformation("Maintenance mode active — blocking request to {Path}", path);

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers["Retry-After"] = retryAfter.ToString();

        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"error\":\"maintenance\",\"message\":\"{message}\"}}");
        }
        else
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(BuildMaintenancePage(message));
        }
    }

    private static string BuildMaintenancePage(string message) =>
        "<!DOCTYPE html>" +
        "<html lang='fr'><head><meta charset='UTF-8'/>" +
        "<meta name='viewport' content='width=device-width,initial-scale=1'/>" +
        "<title>Site en maintenance</title>" +
        "<style>" +
        "body{margin:0;font-family:'Segoe UI',sans-serif;background:#f4f6f8;display:flex;align-items:center;justify-content:center;min-height:100vh;}" +
        ".card{background:#fff;border-radius:16px;padding:3rem 2.5rem;max-width:480px;width:90%;text-align:center;box-shadow:0 4px 24px rgba(0,0,0,.08);}" +
        ".icon{font-size:3rem;margin-bottom:1rem;}" +
        "h1{color:#be1e2c;font-size:1.8rem;margin:0 0 1rem;}" +
        "p{color:#555;font-size:1rem;line-height:1.6;margin:0;}" +
        ".badge{display:inline-block;margin-top:1.5rem;background:#f4f6f8;color:#be1e2c;font-size:.8rem;font-weight:600;padding:.4rem .9rem;border-radius:999px;letter-spacing:.05em;}" +
        "</style></head><body>" +
        "<div class='card'>" +
        "<div class='icon'>🔧</div>" +
        "<h1>Site en maintenance</h1>" +
        $"<p>{System.Net.WebUtility.HtmlEncode(message)}</p>" +
        "<span class='badge'>503 — Service temporairement indisponible</span>" +
        "</div></body></html>";
}

public static class MaintenanceMiddlewareExtensions
{
    public static IApplicationBuilder UseMaintenanceMode(this IApplicationBuilder app)
        => app.UseMiddleware<MaintenanceMiddleware>();
}
