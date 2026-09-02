using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

// We can also include location, device and other information to give us idea where is th user login in from.
public sealed class RefreshToken : BaseEntity
{
    [Key]
    public long LoginSessionId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    [ForeignKey(nameof(UserId))]
    public long UserId { get; set; }

    public bool IsAlive => ExpiryDate > DateTime.UtcNow;

    public User? User { get; set; }
}