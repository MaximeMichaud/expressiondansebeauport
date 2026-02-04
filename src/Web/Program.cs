using Application;
using Domain.Common;
using Domain.Extensions;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Persistence;
using Serilog;
using Web.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddPersistenceServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

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
                .AllowAnyMethod();
        });
});


var app = builder.Build();
await app.Services.InitializeAndSeedDatabase();

var supportedCultures = new[] { "en-CA", "fr-CA" };
app.UseRequestLocalization(options =>
{
    // the order of QueryStringRequestCultureProvider and CookieRequestCultureProvider is switched,
    // so the RequestLocalizationMiddleware looks for the cultures from the cookies first, then query string.
    var questStringCultureProvider = options.RequestCultureProviders[0];
    options.RequestCultureProviders.RemoveAt(0);
    options.RequestCultureProviders.Insert(1, questStringCultureProvider);
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

app.UseStaticFiles();
app.UseRouting();
app.UseCors(corsPolicyBuilder => corsPolicyBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(config => { config.Endpoints.RoutePrefix = "api"; });
app.UseSwaggerGen();

// SPA fallback - serve Vue app for any non-API route
app.MapFallbackToFile("vue/index.html");

app.Run();
