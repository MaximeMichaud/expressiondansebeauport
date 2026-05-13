using Application;
using Application.Interfaces.FileStorage;
using Domain.Common;
using Domain.Extensions;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure;
using Infrastructure.ExternalApis.Local;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.ResponseCompression;
using Persistence;
using Serilog;
using Sidio.Sitemap.Core.Services;
using Web.BackgroundServices;
using Web.Extensions;
using Web.Features.Public.Breadcrumbs;
using Web.Features.Public.Seo;
using Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

const long MaxUploadBytes = 2L * 1024 * 1024 * 1024; // 2 GB

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = MaxUploadBytes;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxUploadBytes;
});

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddPersistenceServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

// User uploads live OUTSIDE wwwroot to protect them from any process that
// rebuilds/wipes static assets (e.g. `vite build` with emptyOutDir).
// The static files middleware below serves the URL prefix /uploads/ from
// this directory, so DB-stored URLs like /uploads/social/foo.webp keep working.
var uploadsRootPath = Path.Combine(builder.Environment.ContentRootPath, "app-data");
Directory.CreateDirectory(Path.Combine(uploadsRootPath, "uploads"));
builder.Services.AddScoped<IFileStorageApiConsumer>(_ => new LocalFileStorageConsumer(uploadsRootPath));

builder.Services.AddSignalR();
builder.Configuration.AddJsonFile("appsettings.local.json", true);
builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(x =>
    {
        x.ExcludeNonFastEndpoints = true;
        x.ShortSchemaNames = true;
    });

// Logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .ReadFrom.Configuration(builder.Configuration)
    .Filter.ByExcluding(x =>
    {
        if (x.Exception?.GetType().Name == null)
            return false;
        var handledErrors = builder.Configuration.GetSection("HandledErrors").Get<List<string>>();
        return handledErrors!.Contains(x.Exception.GetType().Name);
    })
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddHostedService<BackupSchedulerService>();
builder.Services.AddHostedService<AuditLogRetentionScheduler>();
builder.Services.AddDefaultSitemapServices();
builder.Services.AddScoped<IBreadcrumbService, BreadcrumbService>();
builder.Services.AddScoped<ISeoFilesService, SeoFilesService>();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Append("image/svg+xml");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "corsDomains",
        policy =>
        {
            policy.WithOrigins(builder.Configuration.GetSection("CorsDomains")
                    .GetChildren()
                    .Select(c => c.Value)
                    .ToArray()!)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});


var app = builder.Build();
await app.Services.InitializeAndSeedDatabase();

var supportedCultures = new[] { "fr-CA" };
app.UseRequestLocalization(options =>
{
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

app.UseExceptionHandler(c => c.Run(async context =>
{
    var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
    if (exceptionHandler?.Error == null)
        return;

    var responseBody = new SucceededOrNotResponse(false, exceptionHandler.Error.ErrorObject());
    switch (exceptionHandler.Error)
    {
        case ValidationFailureException exception:
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            responseBody = new SucceededOrNotResponse(false, exception.ErrorObjects());
            break;
    }
    await context.Response.WriteAsJsonAsync(responseBody);
}));

app.UseResponseCompression();
app.UseStaticFiles();

// Serve user uploads from the protected app-data/uploads/ directory
// at the URL prefix /uploads/. Kept separate from wwwroot so a Vue build
// cannot wipe user data.
app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(uploadsRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseCors("corsDomains");
app.UseMaintenanceMode();
app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(config => { config.Endpoints.RoutePrefix = "api"; });
app.UseSwaggerGen();

app.MapHub<Web.Hubs.ChatHub>("/hubs/chat");
app.MapSeoFiles();

// SPA fallback - serve Vue app for any non-API route
app.MapFallbackToFile("index.html");

app.Run();
