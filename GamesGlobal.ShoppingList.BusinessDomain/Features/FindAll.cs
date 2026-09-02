using System;
using System.Linq.Expressions;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;

namespace GamesGlobal.ShoppingList.BusinessDomain.Features;

public sealed class FindAll<TEntity> : Specification<TEntity>
    where TEntity : BaseEntity
{
    public FindAll()
    {
    }

    public override Expression<Func<TEntity, bool>> ToExpression()
    {
        return entity => true;
    }
}
