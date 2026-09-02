using System;
using System.Linq.Expressions;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;

public sealed class FindUserByUserCode : Specification<User>
{
    private readonly Guid _userCode;

    public FindUserByUserCode(Guid userCode)
    {
        _userCode = userCode;
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.UserCode == _userCode;
    }
}
