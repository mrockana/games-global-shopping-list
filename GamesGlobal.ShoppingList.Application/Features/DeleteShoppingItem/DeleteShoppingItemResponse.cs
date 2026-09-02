using System;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.Application.Features.DeleteShoppingItem;

public sealed record class DeleteShoppingItemResponse(long ShoppingItemId, Guid UserCode, string Name, string? Description)
{
}

public static class DeleteShoppingItemResponseExtensions
{
    public static DeleteShoppingItemResponse ToDeleteShoppingItemResponse(this ShoppingItem entity)
    {
        return new DeleteShoppingItemResponse(
            ShoppingItemId: entity.ShoppingItemId,
            UserCode: entity.UserCode,
            Name: entity.Name!,
            Description: entity.Description);
    }
}
