using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;

public interface IApplicationRepository
{
    void Delete<TEntity>(TEntity entity)
        where TEntity : BaseEntity;

    void DeleteRange<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : BaseEntity;

    Task<IList<TEntity>> GetAsync<TEntity>(Specification<TEntity> criteria, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    Task<TEntity?> GetSingleAsync<TEntity>(Specification<TEntity> criteria, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    Task<int> CountAsync<TEntity>(Specification<TEntity> criteria, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

    void Insert<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : BaseEntity;

    TEntity Insert<TEntity>(TEntity entity)
        where TEntity : BaseEntity;

    void UseTransaction(IDbTransaction transaction);

    IDbTransaction BeginTransaction();

    ValueTask<int> SaveAsync(CancellationToken cancellationToken = default);

    bool SavedSuccessful(int saveResult);
}
