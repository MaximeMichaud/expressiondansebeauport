using System.Reflection;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Web.Dtos;

namespace Web.Features.Admins.SiteHealth;

public class GetSiteHealthEndpoint : EndpointWithoutRequest<SiteHealthDto>
{
    private readonly GarneauTemplateDbContext _context;
    private readonly IConfiguration _configuration;

    public GetSiteHealthEndpoint(GarneauTemplateDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public override void Configure()
    {
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
            if (!await _context.Database.CanConnectAsync(ct))
            {
                checks.Add(new HealthCheckDto { Name = "Base de données", Status = "Critical", Message = "Connexion impossible" });
            }
            else
            {
                var connection = _context.Database.GetDbConnection();
                var wasOpen = connection.State == System.Data.ConnectionState.Open;
                if (!wasOpen) await connection.OpenAsync(ct);

                string? dbVersion = null;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT version()";
                    dbVersion = (string?)await command.ExecuteScalarAsync(ct);
                }

                if (!wasOpen) await connection.CloseAsync();

                // "PostgreSQL 18.0 on x86_64-pc-linux-musl, compiled by..." → "PostgreSQL 18.0"
                var shortVersion = dbVersion?.Split(" on ")[0];

                checks.Add(new HealthCheckDto { Name = "Base de données", Status = "Good", Message = "Connectée", Details = shortVersion });
            }
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
        var appVersion = (Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute)?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "Inconnue";
        checks.Add(new HealthCheckDto
        {
            Name = "Application",
            Status = "Good",
            Message = appVersion,
            Details = $"SE : {Environment.OSVersion}"
        });

        // Memory
        var memoryWarningThresholdMb = _configuration.GetValue<long>("SiteHealth:MemoryWarningThresholdMb", 512);
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        var memoryMb = process.WorkingSet64 / 1024 / 1024;
        checks.Add(new HealthCheckDto
        {
            Name = "Mémoire",
            Status = memoryMb > memoryWarningThresholdMb ? "Warning" : "Good",
            Message = $"{memoryMb} Mo",
            Details = $"Tas GC : {GC.GetTotalMemory(false) / 1024 / 1024} Mo"
        });

        // Entity counts
        try
        {
            var pageCount = await _context.Pages.CountAsync(ct);
            var mediaCount = await _context.MediaFiles.CountAsync(ct);
            var menuCount = await _context.NavigationMenus.CountAsync(ct);
            checks.Add(new HealthCheckDto
            {
                Name = "Contenu",
                Status = "Good",
                Message = $"{pageCount} page{(pageCount != 1 ? "s" : "")}, {mediaCount} média{(mediaCount != 1 ? "s" : "")}, {menuCount} menu{(menuCount != 1 ? "s" : "")}"
            });
        }
        catch (Exception ex)
        {
            checks.Add(new HealthCheckDto { Name = "Contenu", Status = "Critical", Message = "Lecture impossible", Details = ex.Message });
        }

        var overallStatus = checks.Any(c => c.Status == "Critical") ? "Critical"
            : checks.Any(c => c.Status == "Warning") ? "Warning" : "Good";

        await Send.OkAsync(new SiteHealthDto { OverallStatus = overallStatus, Checks = checks }, cancellation: ct);
    }
}
