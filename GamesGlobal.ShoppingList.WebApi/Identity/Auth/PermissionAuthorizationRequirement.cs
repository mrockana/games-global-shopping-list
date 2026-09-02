using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using Microsoft.AspNetCore.Authorization;

namespace GamesGlobal.ShoppingList.WebApi.Identity.Auth;

public sealed class PermissionAuthorizationRequirement : IAuthorizationRequirement
{
    public PermissionAuthorizationRequirement(Permissions permission)
    {
        Permissions = permission;
    }

    public Permissions Permissions { get; }
}
