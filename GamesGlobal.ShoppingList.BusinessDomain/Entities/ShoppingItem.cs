using System;
using System.ComponentModel.DataAnnotations;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;

namespace GamesGlobal.ShoppingList.BusinessDomain.Entities;

public sealed class ShoppingItem : BaseEntity
{
    public ShoppingItem()
    {
    }

    [Key]
    public long ShoppingItemId { get; set; }

    public Guid UserCode { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Description { get; set; }
}