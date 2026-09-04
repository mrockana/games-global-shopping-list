using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using Pgvector;

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

    public Vector? Embeddings { get; set; }

    public ICollection<Document> Documents { get; set; } = new Collection<Document>();
}