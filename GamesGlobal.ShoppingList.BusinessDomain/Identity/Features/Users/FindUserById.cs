using System;
using System.Linq.Expressions;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;

public sealed class FindUserById : Specification<User>
{
    private readonly long _id;

    public FindUserById(long id)
    {
        _id = id;
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.UserId == _id;
    }
}
