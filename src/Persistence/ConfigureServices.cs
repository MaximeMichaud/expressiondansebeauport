using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Interceptors;

namespace Persistence;

public static class ConfigureServices
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureInfrastructureServices(services);

        ConfigureDbContext(services, configuration);

        return services;
    }

    public static async Task InitializeAndSeedDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<GarneauTemplateDbContextInitializer>();
        await initializer.InitialiseAsync();
        await initializer.SeedAsync();
    }

    private static void ConfigureInfrastructureServices(IServiceCollection services)
    {
        services.AddScoped<AuditableAndSoftDeletableEntitySaveChangesInterceptor>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<EntitySaveChangesInterceptor>();
        services.AddScoped<UserSaveChangesInterceptor>();
    }

    private static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GarneauTemplateDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")!,
                optionsBuilder => optionsBuilder
                    .UseNodaTime()
                    .EnableRetryOnFailure()
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                    .MigrationsAssembly(typeof(GarneauTemplateDbContext).Assembly.FullName));
        });

        services.AddScoped<GarneauTemplateDbContextInitializer>();
    }
}
