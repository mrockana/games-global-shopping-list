using System;
using System.Linq.Expressions;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.AddRole;

internal sealed class FindRoleByName : Specification<Role>
{
    private readonly string _name;

    public FindRoleByName(string name)
    {
        _name = name;
    }

    public override Expression<Func<Role, bool>> ToExpression()
    {
        return role => role.Name.ToLower() == _name.ToLower();
    }
}
