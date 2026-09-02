using System;
using System.Collections.Generic;
using System.Linq;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

public static class PermissionPolicyHelper
{
    public const string Prefix = "Permissions";

    public static bool IsValidPolicyName(string? policyName)
    {
        return policyName != null && policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
    }

    public static string GeneratePolicyNameFor(Permissions permissions)
    {
        long permissionValue = (long)permissions;
        return $"{Prefix}{permissionValue.ToString()}";
    }

    public static Permissions GetPermissionsFrom(string policyName)
    {
        var permissionsValue = long.Parse(policyName[Prefix.Length..]!);

        return (Permissions)permissionsValue;
    }

    public static Permissions GetPermissionsFrom(IList<Role> roles)
    {
        List<Permissions> permissions = roles.SelectMany(role => role.RolePermissions!)
                                    .Where(rolePermission => rolePermission is not null)
                                    .Select(rolePermission => rolePermission.Permission)
                                    .Distinct()
                                    .ToList();

        if (permissions.Count == 0)
        {
            return Permissions.None;
        }

        if (permissions.Exists(p => p == Permissions.All))
        {
            return Permissions.All;
        }

        Permissions resultPermissions = Permissions.None;
        foreach (Permissions permission in permissions)
        {
            resultPermissions |= permission;
        }

        return resultPermissions;
    }
}
