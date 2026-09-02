using System.Threading.Tasks;
using GamesGlobal.ShoppingList.BusinessDomain.Identity;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using Microsoft.AspNetCore.Authorization;

namespace GamesGlobal.ShoppingList.WebApi.Identity.Auth;

internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
    {
        var permissionClaim = context.User.FindFirst(
    c => string.Equals(c.Type, IdentityDomainConstants.PermissionsClaimName, System.StringComparison.Ordinal));

        if (permissionClaim == null)
        {
            return Task.CompletedTask;
        }

        if (!long.TryParse(permissionClaim.Value, out long permissionClaimValue))
        {
            return Task.CompletedTask;
        }

        var userPermissions = (Permissions)permissionClaimValue;

        // Checks if logged in user has the required permissions
        if ((long)(userPermissions & requirement.Permissions) != 0)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
