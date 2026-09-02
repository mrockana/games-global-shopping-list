using System.Collections.Generic;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.UpdateRolePermissions;

public sealed record UpdateRolePermissionsResponse(
    long RoleId,
    string Name,
    IList<UpdateRolePermission> Permissions);

public sealed record UpdateRolePermission(Permissions Permission, string PermissionDescription, string PermissionDescriptionShort);
