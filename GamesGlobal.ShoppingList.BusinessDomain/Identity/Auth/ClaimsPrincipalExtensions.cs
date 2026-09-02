using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserEmail(this ClaimsPrincipal principal)
    {
        string? loggedInUserEmail = principal.FindFirstValue(JwtRegisteredClaimNames.Email);

        if (string.IsNullOrWhiteSpace(loggedInUserEmail))
        {
            return string.Empty;
        }

        return loggedInUserEmail;
    }

    public static Guid GetUserCode(this ClaimsPrincipal principal)
    {
        string? userCode = principal.FindFirstValue(IdentityDomainConstants.UserCodeClaimName);

        if (string.IsNullOrWhiteSpace(userCode))
        {
            return Guid.Empty;
        }

        return Guid.Parse(userCode);
    }
}
