using System;
using System.Linq.Expressions;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.BusinessDomain.Features.ShoppingItems;

public sealed class FindShoppingItemByUserCode : Specification<ShoppingItem>
{
    private readonly Guid _userCode;

    public FindShoppingItemByUserCode(Guid userCode)
    {
        _userCode = userCode;
    }

    public override Expression<Func<ShoppingItem, bool>> ToExpression()
    {
        return shoppingItem => shoppingItem.UserCode == _userCode;
    }
}
