using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess;

public interface IIdentityDbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<RolePermission> RolePermissions { get; set; }

    ChangeTracker ChangeTracker { get; }

    DatabaseFacade Database { get; }

    DbSet<TEntity> Set<TEntity>()
    where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
