using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Web.Dtos;

namespace Web.Features.Admins.SiteHealth;

public class GetSiteHealthEndpoint : EndpointWithoutRequest<SiteHealthDto>
{
    private readonly GarneauTemplateDbContext _context;

    public GetSiteHealthEndpoint(GarneauTemplateDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        DontCatchExceptions();
        Get("admin/site-health");
        Roles(Domain.Constants.User.Roles.ADMINISTRATOR);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var checks = new List<HealthCheckDto>();

        // Database connectivity
        try
        {
            await _context.Database.CanConnectAsync(ct);
            checks.Add(new HealthCheckDto { Name = "Database", Status = "Good", Message = "Connected" });
        }
        catch (Exception ex)
        {
            checks.Add(new HealthCheckDto { Name = "Database", Status = "Critical", Message = "Cannot connect", Details = ex.Message });
        }

        // .NET Runtime
        checks.Add(new HealthCheckDto
        {
            Name = ".NET Runtime",
            Status = "Good",
            Message = $"{Environment.Version}",
            Details = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
        });

        // App info
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        checks.Add(new HealthCheckDto
        {
            Name = "Application",
            Status = "Good",
            Message = assembly.GetName().Version?.ToString() ?? "Unknown",
            Details = $"OS: {Environment.OSVersion}"
        });

        // Memory
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var memoryMb = process.WorkingSet64 / 1024 / 1024;
        checks.Add(new HealthCheckDto
        {
            Name = "Memory",
            Status = memoryMb > 1024 ? "Warning" : "Good",
            Message = $"{memoryMb} MB",
            Details = $"GC Total: {GC.GetTotalMemory(false) / 1024 / 1024} MB"
        });

        // Entity counts
        var pageCount = _context.Pages.Count();
        var mediaCount = _context.MediaFiles.Count();
        var menuCount = _context.NavigationMenus.Count();
        checks.Add(new HealthCheckDto
        {
            Name = "Content",
            Status = "Good",
            Message = $"{pageCount} pages, {mediaCount} media, {menuCount} menus"
        });

        var overallStatus = checks.Any(c => c.Status == "Critical") ? "Critical"
            : checks.Any(c => c.Status == "Warning") ? "Warning" : "Good";

        await Send.OkAsync(new SiteHealthDto { OverallStatus = overallStatus, Checks = checks }, cancellation: ct);
    }
}
