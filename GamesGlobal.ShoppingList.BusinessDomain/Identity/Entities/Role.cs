using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

public sealed class Role : BaseEntity
{
    [Key]
    public long RoleId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Name { get; set; } = string.Empty;

    public ICollection<User>? Users { get; set; } = new Collection<User>();

    public ICollection<RolePermission>? RolePermissions { get; set; } = new Collection<RolePermission>();
}