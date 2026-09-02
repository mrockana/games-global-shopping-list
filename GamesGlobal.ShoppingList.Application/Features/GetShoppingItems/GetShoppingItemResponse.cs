using System;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.Application.Features.GetShoppingItems;

public sealed record class GetShoppingItemResponse(long ShoppingItemId, Guid UserCode, string Name, string? Description)
{
}

public static class GetShoppingItemResponseExtensions
{
    public static GetShoppingItemResponse ToGetShoppingItemResponse(this ShoppingItem entity)
    {
        return new GetShoppingItemResponse(
            UserCode: entity.UserCode,
            ShoppingItemId: entity.ShoppingItemId,
            Name: entity.Name!,
            Description: entity.Description);
    }
}
