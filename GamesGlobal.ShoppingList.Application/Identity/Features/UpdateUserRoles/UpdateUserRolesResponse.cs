using System.Collections.Generic;
namespace GamesGlobal.ShoppingList.Application.Identity.Features.UpdateUserRoles;

public sealed record UpdateUserRolesResponse(
   long UserId,
   IList<UpdateUserRoleRoleResponse> Roles);

public sealed record UpdateUserRoleRoleResponse(long RoleId, string Name);