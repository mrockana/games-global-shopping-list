using System;

namespace GamesGlobal.ShoppingList.Application.Features;

public static class ShoppingItemCacheKeys
{
    public static string ForUser(Guid userCode) => $"shopping-items:{userCode}";
}