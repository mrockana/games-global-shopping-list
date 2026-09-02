using System;
using System.ComponentModel;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

[Flags]
public enum Permissions : long
{
    /// <summary>
    ///  None represents no permissions.
    /// </summary>
    None = 0,

    /// <summary>
    ///  This permission allow the user to manage their own shopping items including, deleting, updating and creating new shopping items.
    /// </summary>
    [Description("Shopping Items Self Read/Write")]
    ShoppingItemsSelfReadWrite = 1,

    /// <summary>
    ///  This permission allow the user to only view their own shopping items, and CANNOT create, update or delete them.
    /// </summary>
    [Description("Shopping Items Self Read Only")]
    ShoppingItemsSelfReadOnly = 2,

    /// <summary>
    ///  This permission allow the user to ONLY VIEW User Roles and Permissions. Cannot MODIFY them.
    /// </summary>
    [Description("User Roles And Permissions Read")]
    UserRolesAndPermissionsReadOnly = 4,

    /// <summary>
    ///  This permission allow the user to View and Modify User Roles and Permissions.
    /// </summary>
    [Description("User Roles And Permissions Read Write")]
    UserRolesAndPermissionsReadWrite = 8,

    /// <summary>
    ///  This is all access super admin privilege, only god has more privilege than super admin.
    /// </summary>
    [Description("All")]
    All = ~None,
}
