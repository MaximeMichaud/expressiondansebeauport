using Domain.Entities.Identity;
using Domain.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Persistence.Extensions;

namespace Persistence.Interceptors;

/// <summary>
/// Duplicated from AuditableEntitySaveChangeInterceptor. Will probably have more stuff here.
/// </summary>
public class UserSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var actionMadeBy = _httpContextAccessor.HttpContext.GetUserEmail() ?? "application";
        foreach (var entry in context.ChangeTracker.Entries<User>())
        {
            if (entry.Entity.Deleted.HasValue && string.IsNullOrWhiteSpace(entry.Entity.DeletedBy))
                entry.Entity.SetDeletedBy(actionMadeBy);

            if (entry.State == EntityState.Added)
                entry.Entity.UpdateCreated(InstantHelper.GetLocalNow(), actionMadeBy);

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                entry.Entity.UpdateLastModified(InstantHelper.GetLocalNow(), actionMadeBy);

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.SoftDelete(actionMadeBy);
            }
        }
    }
}