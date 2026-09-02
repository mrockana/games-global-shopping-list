using System;
using System.Linq.Expressions;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Roles;

public sealed class FindRoleById : Specification<Role>
{
    private readonly long _id;

    public FindRoleById(long id)
    {
        _id = id;
    }

    public long Id => _id;

    public override Expression<Func<Role, bool>> ToExpression()
    {
        return role => role.RoleId == _id;
    }
}
