using System.Diagnostics;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class AuditableEntityInterceptor(IUser user,TimeProvider datetime):SaveChangesInterceptor
{
    private readonly IUser _user = user;
    private readonly TimeProvider _datetime = datetime;

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
        var UtcNow=_datetime.GetUtcNow();

        if(context is null)
         return;

        foreach(var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if(entry.State==EntityState.Added||entry.State==EntityState.Modified||entry.HasChangedOwnedEntities())
            {
                if(entry.State==EntityState.Added)
                {
                  entry.Entity.CreatedBy=_user.Id;
                  entry.Entity.CreatedAtUtc=UtcNow;   
                }
                entry.Entity.LastModifiedBy=_user.Id;
                entry.Entity.LastModifiedUtc = UtcNow;  
            }

            foreach(var ownedEntry in entry.References)
            {
                 if (ownedEntry.TargetEntry is { Entity: AuditableEntity ownedEntity } 
                    && ownedEntry.TargetEntry.State is EntityState.Added or EntityState.Modified)
                {
                    if(ownedEntry.TargetEntry.State==EntityState.Added)
                    {
                          entry.Entity.CreatedBy=_user.Id;
                          entry.Entity.CreatedAtUtc=UtcNow;   
                    }
                    
                           entry.Entity.LastModifiedBy=_user.Id;
                           entry.Entity.LastModifiedUtc = UtcNow;  
                    
                }
            }
        }

    }
    
}
public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry?.Metadata.IsOwned() == true &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}