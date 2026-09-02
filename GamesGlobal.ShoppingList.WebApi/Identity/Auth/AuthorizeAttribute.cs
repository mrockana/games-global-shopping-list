using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

namespace GamesGlobal.ShoppingList.WebApi.Identity.Auth;

public sealed class AuthorizeAttribute : Microsoft.AspNetCore.Authorization.AuthorizeAttribute
{
    public AuthorizeAttribute()
    {
    }

    public AuthorizeAttribute(string policy)
        : base(policy)
    {
    }

    public AuthorizeAttribute(Permissions permission)
    {
        Permissions = permission;
    }

    public Permissions Permissions
    {
        get
        {
            return !string.IsNullOrEmpty(Policy)
                ? PermissionPolicyHelper.GetPermissionsFrom(Policy)
                : Permissions.None;
        }
        set
        {
            Policy = value != Permissions.None
                ? PermissionPolicyHelper.GeneratePolicyNameFor(value)
                : string.Empty;
        }
    }
}