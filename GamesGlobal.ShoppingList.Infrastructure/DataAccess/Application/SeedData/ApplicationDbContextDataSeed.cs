using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.SeedData;

internal static class ApplicationDbContextDataSeed
{
    private const string ShoppingItemsResourceName = "GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.SeedData.ShoppingItems.json";

    internal static void AddApplicationDbContextDataSeed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShoppingItem>().HasData(LoadShoppingItems());
    }

    private static IEnumerable<ShoppingItem> LoadShoppingItems()
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ShoppingItemsResourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{ShoppingItemsResourceName}' was not found.");
        using var reader = new StreamReader(stream);

        return JsonSerializer.Deserialize<List<ShoppingItem>>(reader.ReadToEnd())
            ?? throw new InvalidOperationException("Shopping item seed data could not be deserialized.");
    }
}
