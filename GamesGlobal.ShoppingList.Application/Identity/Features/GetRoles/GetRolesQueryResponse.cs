using System.Collections.Generic;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.GetRoles;

public sealed record GetRolesQueryResponse(
    long RoleId,
    string Name,
    IList<PermissionResponse> Permissions);

public sealed record PermissionResponse(Permissions Permission, string PermissionDescription, string PermissionDescriptionShort, bool Enabled);