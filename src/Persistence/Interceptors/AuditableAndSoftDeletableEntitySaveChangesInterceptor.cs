using Domain.Common;
using Domain.Entities;
using Domain.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Persistence.Extensions;

namespace Persistence.Interceptors;

public class AuditableAndSoftDeletableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditableAndSoftDeletableEntitySaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SanitizeEntities(eventData.Context);
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SanitizeEntities(eventData.Context);
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var actionMadeBy = _httpContextAccessor.HttpContext.GetUserEmail() ?? "application";
        foreach (var entry in context.ChangeTracker.Entries<AuditableAndSoftDeletableEntity>())
        {
            if (entry.Entity.Deleted.HasValue && string.IsNullOrWhiteSpace(entry.Entity.DeletedBy))
                entry.Entity.DeletedBy = actionMadeBy;
            
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = actionMadeBy;
                entry.Entity.Created = InstantHelper.GetLocalNow();
            } 

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                entry.Entity.LastModifiedBy = actionMadeBy;
                entry.Entity.LastModified = InstantHelper.GetLocalNow();
            }
            
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.SoftDelete(actionMadeBy);
            }
        }
    }
    
    private void SanitizeEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<ISanitizable>())
        {
            entry.Entity.SanitizeForSaving();
        }
    }
}