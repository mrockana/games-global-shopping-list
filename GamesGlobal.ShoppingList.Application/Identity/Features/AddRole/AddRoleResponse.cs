using System.Collections.Generic;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.AddRole;

public sealed record AddRoleResponse(
    long RoleId,
    string Name,
    IList<AddRolePermissionResponse> Permissions);

public sealed record AddRolePermissionResponse(Permissions Permission, string PermissionDescription, string PermissionDescriptionShort);
