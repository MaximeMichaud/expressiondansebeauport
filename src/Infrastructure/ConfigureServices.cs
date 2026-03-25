using System.Text;
using Application.Interfaces.Services;
using Domain.Entities.Identity;
using Domain.Repositories;
using Infrastructure.Mailing;
using Infrastructure.Repositories.Admins;
using Infrastructure.Repositories.Authentication;
using Infrastructure.Repositories.Media;
using Infrastructure.Repositories.Groups;
using Infrastructure.Repositories.Messaging;
using Infrastructure.Repositories.Posts;
using Infrastructure.Repositories.Members;
using Infrastructure.Repositories.Menus;
using Infrastructure.Repositories.Pages;
using Infrastructure.Repositories.Users;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IMediaFileRepository, MediaFileRepository>();
        services.AddScoped<ISiteSettingsRepository, Infrastructure.Repositories.SiteSettings.SiteSettingsRepository>();
        services.AddScoped<INavigationMenuRepository, NavigationMenuRepository>();
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IEmailConfirmationCodeRepository, EmailConfirmationCodeRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IPollRepository, PollRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        services.AddScoped<IBackupService>(sp =>
        {
            var context = sp.GetRequiredService<GarneauTemplateDbContext>();
            var config = sp.GetRequiredService<IConfiguration>();
            var logger = sp.GetRequiredService<ILogger<Services.BackupService>>();
            var env = sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();
            var webRootPath = Path.Combine(env.ContentRootPath, "wwwroot");
            return new Services.BackupService(context, config, logger, webRootPath);
        });
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

                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
    }
}