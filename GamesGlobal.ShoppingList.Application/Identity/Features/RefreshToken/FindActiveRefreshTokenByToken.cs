using System;
using System.Linq.Expressions;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.RefreshToken;

public sealed class FindActiveRefreshTokenByToken : Specification<BusinessDomain.Identity.Entities.RefreshToken>
{
    private readonly string _refreshToken;

    public FindActiveRefreshTokenByToken(string refreshToken)
    {
        _refreshToken = refreshToken;
    }

    public override Expression<Func<BusinessDomain.Identity.Entities.RefreshToken, bool>> ToExpression()
    {
        return loginSession => loginSession.Token == _refreshToken &&
                                loginSession.ExpiryDate > DateTime.UtcNow;
    }
}
