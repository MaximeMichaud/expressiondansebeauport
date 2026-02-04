using System.Text;
using Application.Interfaces.FileStorage;
using Application.Interfaces.Services;
using Domain.Entities.Identity;
using Domain.Repositories;
using Infrastructure.ExternalApis.Azure;
using Infrastructure.ExternalApis.Azure.Consumers;
using Infrastructure.ExternalApis.Azure.Http;
using Infrastructure.Mailing;
using Infrastructure.Repositories.Admins;
using Infrastructure.Repositories.Authentication;
using Infrastructure.Repositories.Books;
using Infrastructure.Repositories.Members;
using Infrastructure.Repositories.Users;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using ScottBrady91.AspNetCore.Identity;

namespace Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigureInfrastructureServices(services);
        ConfigureFormsServices(services);

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ConfigureServices).Assembly));

        MailingInitializer.Configure(services, configuration);

        ConfigureAuthentication(services, configuration);

        return services;
    }

    private static void ConfigureFormsServices(IServiceCollection services)
    {
        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
        });

        services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
            x.MultipartHeadersLengthLimit = int.MaxValue;
        });
    }

    private static void ConfigureInfrastructureServices(IServiceCollection services)
    {
        services.AddSingleton<IHttpContextUserService, HttpContextUserService>();

        services.AddScoped<IAdministratorRepository, AdministratorRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IFileStorageApiConsumer, AzureBlobApiConsumer>();
        services.AddScoped<IAzureApiHttpClient, AzureApiHttpClient>();
        services.AddScoped<IAzureBlobWrapper, AzureBlobWrapper>();
    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<User>(options =>
            {
                options.Stores.MaxLengthForKeys = 128;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 10;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 6;
            })
            .AddRoles<Role>()
            .AddRoleManager<RoleManager<Role>>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<GarneauTemplateDbContext>()
            .AddSignInManager<SignInManager<User>>();

        // Add and configure Argon2 password hasher
        services.AddScoped<IPasswordHasher<User>, Argon2PasswordHasher<User>>();
        services.Configure<Argon2PasswordHasherOptions>(options =>
        {
            options.Strength = Argon2HashStrength.Interactive;
        });

        var tokenSigningKey = configuration.GetSection("JwtToken:SecretKey").Value!;
        var issuer = configuration.GetSection("JwtToken:Issuer").Value;
        var audience = configuration.GetSection("JwtToken:Audience").Value;
        services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenSigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(10)
                };
            });
    }
}