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

    internal static void AddApplicationDbContextDataSeed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShoppingItem>().HasData(LoadShoppingItems());
    }

    internal static async Task PostMigrationEmbeddingsSeeding(
        this ApplicationDbContext context,
        IEmbeddingService embeddingService,
        CancellationToken cancellationToken = default)
    {
        var ids = LoadShoppingItems().Select(p => p.ShoppingItemId);
        List<ShoppingItem> itemsNeedingEmbeddings = await context.ShoppingItems
            .Where(item => item.Embeddings == null && ids.Contains(item.ShoppingItemId))

            .ToListAsync(cancellationToken);

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
