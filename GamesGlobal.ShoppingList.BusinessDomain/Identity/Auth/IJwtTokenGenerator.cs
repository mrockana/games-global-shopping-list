using System;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

public interface IJwtTokenGenerator
{
    string Generate(string username, Guid userCode, Permissions permissions);
}