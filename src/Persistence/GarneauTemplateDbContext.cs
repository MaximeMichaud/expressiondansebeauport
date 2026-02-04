using System.Reflection;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Authentication;
using Domain.Entities.Books;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;
using Persistence.Interceptors;

namespace Persistence;

public class GarneauTemplateDbContext : IdentityDbContext<User, Role, Guid,
    IdentityUserClaim<Guid>, UserRole,
    IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    private readonly AuditableAndSoftDeletableEntitySaveChangesInterceptor
        _auditableAndSoftDeletableEntitySaveChangesInterceptor = null!;

    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor = null!;
    private readonly UserSaveChangesInterceptor _userSaveChangesInterceptor = null!;
    private readonly EntitySaveChangesInterceptor _entitySaveChangesInterceptor = null!;

    public GarneauTemplateDbContext(
        DbContextOptions<GarneauTemplateDbContext> options,
        AuditableAndSoftDeletableEntitySaveChangesInterceptor auditableAndSoftDeletableEntitySaveChangesInterceptor,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor,
        UserSaveChangesInterceptor userSaveChangesInterceptor,
        EntitySaveChangesInterceptor entitySaveChangesInterceptor)
        : base(options)
    {
        _auditableAndSoftDeletableEntitySaveChangesInterceptor = auditableAndSoftDeletableEntitySaveChangesInterceptor;
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        _userSaveChangesInterceptor = userSaveChangesInterceptor;
        _entitySaveChangesInterceptor = entitySaveChangesInterceptor;
    }

    public DbSet<Administrator> Administrators { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    public GarneauTemplateDbContext()
    {
    }

    public GarneauTemplateDbContext(DbContextOptions<GarneauTemplateDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Global query to prevent loading soft-deleted entities
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                continue;

            if (entityType.ClrType == typeof(User))
                continue;

            entityType.AddSoftDeleteQueryFilter();
        }

        builder.Entity<Book>().Property(b => b.Price).HasPrecision(18, 2);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(
            _auditableAndSoftDeletableEntitySaveChangesInterceptor,
            _auditableEntitySaveChangesInterceptor,
            _userSaveChangesInterceptor,
            _entitySaveChangesInterceptor);
    }

    public async Task<int> SaveChangesAsync(CancellationToken? cancellationToken = null)
    {
        return await base.SaveChangesAsync(cancellationToken ?? CancellationToken.None);
    }
}