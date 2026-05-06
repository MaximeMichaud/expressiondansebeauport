using System.Text;
using Application.Interfaces.Imaging;
using Application.Interfaces.Services;
using Application.Services.Push;
using Domain.Entities.Identity;
using Domain.Repositories;
using Infrastructure.Imaging;
using Infrastructure.Mailing;
using Infrastructure.Repositories.Admins;
using Infrastructure.Repositories.AuditLogs;
using Infrastructure.Services.Push;
using Infrastructure.Repositories.Authentication;
using Infrastructure.Repositories.Media;
using Infrastructure.Repositories.Groups;
using Infrastructure.Repositories.JoinRequests;
using Infrastructure.Repositories.Messaging;
using Infrastructure.Repositories.Notifications;
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
        ConfigureInfrastructureServices(services, configuration);
        ConfigureFormsServices(services);

        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ConfigureServices).Assembly));

        MailingInitializer.Configure(services, configuration);

        ConfigureAuthentication(services, configuration);

        return services;
    }

    private static void ConfigureFormsServices(IServiceCollection services)
    {
        const long maxMultipartBodySize = 100 * 1024 * 1024;

        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = maxMultipartBodySize;
        });

        services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = 1024 * 1024;
            x.MultipartBodyLengthLimit = maxMultipartBodySize;
            x.MultipartHeadersLengthLimit = 64 * 1024;
        });
    }

    private static void ConfigureInfrastructureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHttpContextUserService, HttpContextUserService>();

        services.AddScoped<IImageProcessor, SkiaSharpImageProcessor>();

        services.AddScoped<IAdministratorRepository, AdministratorRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IMediaFileRepository, MediaFileRepository>();
        services.AddScoped<ISiteSettingsRepository, Infrastructure.Repositories.SiteSettings.SiteSettingsRepository>();
        services.AddScoped<INavigationMenuRepository, NavigationMenuRepository>();
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<IPageRevisionRepository, PageRevisionRepository>();
        services.AddScoped<IPreviewTokenRepository, PreviewTokenRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IEmailConfirmationCodeRepository, EmailConfirmationCodeRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IPollRepository, PollRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IJoinRequestRepository, JoinRequestRepository>();
        services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
        services.AddScoped<INotificationPreferencesRepository, NotificationPreferencesRepository>();
        services.AddSingleton<IPushSenderClient, WebPushSenderClient>();

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
                        var path = context.HttpContext.Request.Path;
                        var queryToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(queryToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = queryToken;
                            return Task.CompletedTask;
                        }

                        var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization")
                            && !string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]);
                        if (!hasAuthHeader && context.Request.Cookies.TryGetValue("accessToken", out var cookieToken)
                            && !string.IsNullOrWhiteSpace(cookieToken))
                        {
                            context.Token = cookieToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
    }
}
