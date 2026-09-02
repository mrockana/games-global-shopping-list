using System;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Common;

internal sealed class AuditFieldsModificationInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditFields(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditFields(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditFields(DbContext dbContext)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is BaseEntity entityCreated)
                    {
                        entityCreated.Created = DateTime.UtcNow;
                    }

                    break;
                case EntityState.Modified:
                    if (entry.Entity is BaseEntity entityModified)
                    {
                        entityModified.Modified = DateTime.UtcNow;
                    }

                    break;
            }
        }
    }
}
