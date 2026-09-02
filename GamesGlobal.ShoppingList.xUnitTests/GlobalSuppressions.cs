// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "xUnit1045:Avoid using TheoryData type arguments that might not be serializable", Justification = "Tested the following and it works", Scope = "member", Target = "~M:GamesGlobal.ShoppingList.xUnitTests.Application.Identity.Features.UpdateUserRolesCommandHandlerTests.Handle_RoleIdsIsNullOrEmpty_RemovesAllRoles(System.Collections.Generic.IList{System.Int64})~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Usage", "xUnit1042:The member referenced by the MemberData attribute returns untyped data rows", Justification = "Tested the following and it works", Scope = "member", Target = "~M:GamesGlobal.ShoppingList.xUnitTests.Application.Identity.Features.AddRoleCommandHandlerTests.Validation_PermissionsIsNullOrEmpty_ReturnsInvalid(System.Collections.Generic.IList{GamesGlobal.ShoppingList.Application.Identity.Features.AddRole.AdddRolePermission})")]
