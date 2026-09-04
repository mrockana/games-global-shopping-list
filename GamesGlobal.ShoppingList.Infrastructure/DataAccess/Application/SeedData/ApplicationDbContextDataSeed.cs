using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common.Embeddings;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.SeedData;

internal static class ApplicationDbContextDataSeed
{
    private const string ShoppingItemsResourceName = "GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.SeedData.ShoppingItems.json";

    internal static async Task PostMigrationEmbeddingsSeeding(
        this ApplicationDbContext context,
        IEmbeddingService embeddingService,
        CancellationToken cancellationToken = default)
    {
        List<ShoppingItem> seedingShoppingItems = LoadShoppingItems().ToList();

        Dictionary<long, ShoppingItem> existingItems = await context.ShoppingItems
            .Where(item => seedingShoppingItems.Select(si => si.ShoppingItemId).Contains(item.ShoppingItemId))
            .ToDictionaryAsync(item => item.ShoppingItemId, cancellationToken);
        List<ShoppingItem> itemsNeedingEmbeddings = existingItems.Values
            .Where(item => item.Embeddings is null)
            .ToList();

        foreach (ShoppingItem shoppingItem in seedingShoppingItems)
        {
            if (existingItems.ContainsKey(shoppingItem.ShoppingItemId))
            {
                continue;
            }

            context.ShoppingItems.Add(shoppingItem);
            itemsNeedingEmbeddings.Add(shoppingItem);
        }

        if (itemsNeedingEmbeddings.Count == 0)
        {
            return;
        }

        IReadOnlyList<Pgvector.Vector> embeddings = await embeddingService.GenerateAsync(
            itemsNeedingEmbeddings.Select(item => $"{item.Name} {item.Description}").ToList(),
            cancellationToken);

        for (int index = 0; index < itemsNeedingEmbeddings.Count; index++)
        {
            itemsNeedingEmbeddings[index].Embeddings = embeddings[index];
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<ShoppingItem> LoadShoppingItems()
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ShoppingItemsResourceName)
            ?? throw new DomainDependencyException($"Embedded resource '{ShoppingItemsResourceName}' was not found.");
        using var reader = new StreamReader(stream);

        return JsonSerializer.Deserialize<List<ShoppingItem>>(reader.ReadToEnd())
            ?? throw new DomainDependencyException("Shopping item seed data could not be deserialized.");
    }
}
