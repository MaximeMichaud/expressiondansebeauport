using Application.Interfaces.Services;
using Domain.Constants.User;
using Domain.Entities;
using Domain.Entities.Identity;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Persistence.Interceptors;
using ScottBrady91.AspNetCore.Identity;
using Tests.Common.Fixtures;

namespace Tests.Infrastructure.Fixtures;

public class PersistenceTestFixture : TestFixture, IDisposable
{
    public readonly GarneauTemplateDbContext DbContext;
    public readonly UserManager<User> UserManager;
    public readonly RoleManager<Role> RoleManager;

    public PersistenceTestFixture()
    {
        // Create test database and apply migrations
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        serviceCollection.AddSingleton<IHttpContextUserService, HttpContextUserService>();
        serviceCollection.AddScoped<EntitySaveChangesInterceptor>();
        serviceCollection.AddScoped<AuditableAndSoftDeletableEntitySaveChangesInterceptor>();
        serviceCollection.AddScoped<AuditableEntitySaveChangesInterceptor>();
        serviceCollection.AddScoped<UserSaveChangesInterceptor>();

        serviceCollection.AddDbContext<GarneauTemplateDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")!,
                optionsBuilder => optionsBuilder
                    .UseNodaTime()
                    .EnableRetryOnFailure()
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                    .MigrationsAssembly(typeof(GarneauTemplateDbContext).Assembly.FullName));
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        DbContext = serviceCollection.BuildServiceProvider().GetService<GarneauTemplateDbContext>()!;
        if (DbContext == null)
            throw new Exception("DbContext cannot be null.");

        DbContext.Database.EnsureDeleted();
        DbContext.Database.Migrate();

        serviceCollection.AddDataProtection();

        serviceCollection.AddIdentityCore<User>(options =>
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

        serviceCollection.AddScoped<IPasswordHasher<User>, Argon2PasswordHasher<User>>();
        serviceCollection.Configure<Argon2PasswordHasherOptions>(options =>
        {
            options.Strength = Argon2HashStrength.Interactive;
        });

        // Set Identity related managers
        UserManager = serviceCollection.BuildServiceProvider().GetService<UserManager<User>>()!;
        RoleManager = serviceCollection.BuildServiceProvider().GetService<RoleManager<Role>>()!;

        SeedRoles();
    }

    private void SeedRoles()
    {
        var role = BuildRole(Roles.ADMINISTRATOR);
        DbContext.Roles.Add(role);
        DbContext.SaveChanges();
        AnyRoleId = role.Id;
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        UserManager.Dispose();
        RoleManager.Dispose();
    }

    public Role FindRoleWithName(string roleName)
    {
        return DbContext.Roles.First(x => x.Name == roleName);
    }

    public async Task<User> GivenUserInDatabase()
    {
        var user = UserBuilder.WithEmail(GenerateEmail()).AsNotDeleted().Build();
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        if (DbContext.UserRoles.Any(x => x.UserId == user.Id && x.RoleId == AnyRoleId))
            return user;

        DbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = AnyRoleId });
        await DbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User> GivenUserInUserManager()
    {
        var user = UserBuilder.WithEmail(GenerateEmail()).AsNotDeleted().Build();
        await UserManager.CreateAsync(user);
        await UserManager.AddPasswordAsync(user, "Qwerty123!");

        if (DbContext.UserRoles.Any(x => x.UserId == user.Id && x.RoleId == AnyRoleId))
            return user;

        DbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = AnyRoleId });
        await DbContext.SaveChangesAsync();
        return user;
    }

    public async Task<Member> GivenMember(string? email = null)
    {
        var member = MemberBuilder.WithEmail(email ?? GenerateEmail()).Build();
        DbContext.Members.Add(member);
        await DbContext.SaveChangesAsync();
        return member;
    }
}