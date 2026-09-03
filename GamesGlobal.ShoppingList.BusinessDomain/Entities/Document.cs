using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;

namespace GamesGlobal.ShoppingList.BusinessDomain.Entities;

public sealed class Document : BaseEntity
{
    [Key]
    public long DocumentId { get; set; }

    [Required]
    public string? MimeType { get; set; }

    [Required]
    public string? Url { get; set; }

    [Required]
    public int Size { get; set; }

    [Required]
    public string? Name { get; set; }

    public ICollection<ShoppingItem> ShoppingItems { get; set; } = new Collection<ShoppingItem>();
}