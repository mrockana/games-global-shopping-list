using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Identity;

internal sealed class IdentityRepository : IIdentityRepository
{
    private readonly IIdentityDbContext _context;

    public IdentityRepository(IIdentityDbContext context)
    {
        _context = context;
    }

    public void Delete<TEntity>(TEntity entity)
        where TEntity : BaseEntity
    {
        _context.Set<TEntity>().Remove(entity);
    }

    public void DeleteRange<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : BaseEntity
    {
        _context.Set<TEntity>().RemoveRange(entities);
    }

    public async Task<IList<TEntity>> GetAsync<TEntity>(Specification<TEntity> criteria, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        var query = GetQuery(_context.Set<TEntity>().AsQueryable(), criteria);
        return await query.ToListAsync(cancellationToken);
    }

    public Task<TEntity?> GetSingleAsync<TEntity>(Specification<TEntity> criteria, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        var query = GetQuery(_context.Set<TEntity>().AsQueryable(), criteria);
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<int> CountAsync<TEntity>(Specification<TEntity> criteria, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity
    {
        var query = GetQuery(_context.Set<TEntity>().AsQueryable(), criteria);
        return query.CountAsync(cancellationToken);
    }

    public void Insert<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : BaseEntity
    {
        _context.Set<TEntity>().AddRange(entities);
    }

    public TEntity Insert<TEntity>(TEntity entity)
        where TEntity : BaseEntity
    {
        var entityResult = _context.Set<TEntity>().Add(entity);
        return entityResult.Entity;
    }

    public void UseTransaction(IDbTransaction transaction)
    {
        _context.Database.UseTransaction((DbTransaction)transaction);
    }

    public IDbTransaction BeginTransaction()
    {
        var transaction = _context.Database.BeginTransaction();
        return transaction.GetDbTransaction();
    }

    public async ValueTask<int> SaveAsync(CancellationToken cancellationToken = default)
    {
        if (_context.ChangeTracker.HasChanges())
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        return 1;
    }

    public bool SavedSuccessful(int saveResult)
    {
        if (saveResult <= 0)
        {
            return false;
        }

        return true;
    }

    private static IQueryable<TEntity> GetQuery<TEntity>(IQueryable<TEntity> query, Specification<TEntity> criteria)
        where TEntity : BaseEntity
    {
        query = criteria.GetQuery(query);

        return query;
    }
}
