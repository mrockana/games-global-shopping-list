using System;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.Application.Features.CreateShoppingItem;

public sealed record class CreateShoppingItemResponse(Guid UserCode, long ShoppingItemId, string Name, string? Description)
{
}

public static class CreateShoppingItemResponseExtensions
{
    public static CreateShoppingItemResponse ToCreateShoppingItemResponse(this ShoppingItem entity)
    {
        return new CreateShoppingItemResponse(
            UserCode: entity.UserCode,
            ShoppingItemId: entity.ShoppingItemId,
            Name: entity.Name!,
            Description: entity.Description);
    }
}