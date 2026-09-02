using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.Application.Features.UpdateShoppingItemCommand;

public sealed record class UpdateShoppingItemResponse(long ShoppingItemId, string Name, string? Description)
{
}

public static class UpdateShoppingItemResponseExtensions
{
    public static UpdateShoppingItemResponse ToUpdateShoppingItemResponse(this ShoppingItem entity)
    {
        return new UpdateShoppingItemResponse(
            ShoppingItemId: entity.ShoppingItemId,
            Name: entity.Name!,
            Description: entity.Description);
    }
}
