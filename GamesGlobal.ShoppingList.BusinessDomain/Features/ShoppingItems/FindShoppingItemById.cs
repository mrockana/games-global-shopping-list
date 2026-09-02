using System;
using System.Linq.Expressions;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.BusinessDomain.Features.ShoppingItems;

public sealed class FindShoppingItemById : Specification<ShoppingItem>
{
    private readonly long _id;

    public FindShoppingItemById(long id)
    {
        _id = id;
    }

    public override Expression<Func<ShoppingItem, bool>> ToExpression()
    {
        return shoppingItem => shoppingItem.ShoppingItemId == _id;
    }
}
