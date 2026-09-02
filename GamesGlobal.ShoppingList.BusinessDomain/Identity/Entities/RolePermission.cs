using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

public sealed class RolePermission : BaseEntity
{
    [Key]
    public long RolePermissionId { get; set; }

    [Required]
    public Permissions Permission { get; set; } = Permissions.None;

    [Required]
    public required string PermissionName { get; set; }

    [Required]
    [ForeignKey(nameof(RoleId))]
    public long RoleId { get; set; }

    public Role? Role { get; set; }
}