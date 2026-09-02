using System;
using System.Linq.Expressions;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;

public sealed class FindUserByEmail : Specification<User>
{
    private readonly string _email;

    public FindUserByEmail(string email)
    {
        _email = email?.ToLowerInvariant()!;
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.Email == _email;
    }
}
