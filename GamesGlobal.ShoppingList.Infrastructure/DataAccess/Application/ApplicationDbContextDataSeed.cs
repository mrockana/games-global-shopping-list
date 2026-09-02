using System;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application;

internal static class ApplicationDbContextDataSeed
{
    internal static void AddApplicationDbContextDataSeed(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShoppingItem>().HasData(new ShoppingItem
        {
            ShoppingItemId = 1,
            UserCode = Guid.Parse("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f"),
            Name = "Pants",
            Description = "Blue denim jean, medium size.",
            Created = new DateTime(2025, 04, 1, 12, 0, 0, DateTimeKind.Utc),
        });

        modelBuilder.Entity<ShoppingItem>().HasData(new ShoppingItem
        {
            ShoppingItemId = 2,
            UserCode = Guid.Parse("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f"),
            Name = "Shirt",
            Description = "White plain shirt, medium size.",
            Created = new DateTime(2025, 04, 1, 12, 0, 0, DateTimeKind.Utc),
        });
    }
}
