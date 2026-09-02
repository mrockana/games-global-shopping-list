using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;

public interface IApplicationDbContext
{
    DbSet<ShoppingItem> ShoppingItems { get; }

    ChangeTracker ChangeTracker { get; }

    DatabaseFacade Database { get; }

    DbSet<TEntity> Set<TEntity>()
    where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
