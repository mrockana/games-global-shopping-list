namespace GamesGlobal.ShoppingList.BusinessDomain.Entities;

public sealed class ShoppingItemDocument
{
    public long ShoppingItemId { get; set; }

    public long DocumentId { get; set; }

    public ShoppingItem? ShoppingItem { get; set; }

    public Document? Document { get; set; }
}
