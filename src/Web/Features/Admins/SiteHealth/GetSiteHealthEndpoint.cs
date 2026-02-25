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
            checks.Add(new HealthCheckDto { Name = "Base de données", Status = "Good", Message = "Connectée" });
        }
        catch (Exception ex)
        {
            checks.Add(new HealthCheckDto { Name = "Base de données", Status = "Critical", Message = "Connexion impossible", Details = ex.Message });
        }

        // .NET Runtime
        checks.Add(new HealthCheckDto
        {
            Name = "Environnement .NET",
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
            Message = assembly.GetName().Version?.ToString() ?? "Inconnue",
            Details = $"SE : {Environment.OSVersion}"
        });

        // Memory
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var memoryMb = process.WorkingSet64 / 1024 / 1024;
        checks.Add(new HealthCheckDto
        {
            Name = "Mémoire",
            Status = memoryMb > 1024 ? "Warning" : "Good",
            Message = $"{memoryMb} Mo",
            Details = $"Tas GC : {GC.GetTotalMemory(false) / 1024 / 1024} Mo"
        });

        // Entity counts
        var pageCount = _context.Pages.Count();
        var mediaCount = _context.MediaFiles.Count();
        var menuCount = _context.NavigationMenus.Count();
        checks.Add(new HealthCheckDto
        {
            Name = "Contenu",
            Status = "Good",
            Message = $"{pageCount} page{(pageCount > 1 ? "s" : "")}, {mediaCount} média{(mediaCount > 1 ? "s" : "")}, {menuCount} menu{(menuCount > 1 ? "s" : "")}"
        });

        var overallStatus = checks.Any(c => c.Status == "Critical") ? "Critical"
            : checks.Any(c => c.Status == "Warning") ? "Warning" : "Good";

        await Send.OkAsync(new SiteHealthDto { OverallStatus = overallStatus, Checks = checks }, cancellation: ct);
    }
}
