using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.EntityConfiguration;
using GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.SeedData;
using Microsoft.EntityFrameworkCore;

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application;

internal sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShoppingItem> ShoppingItems { get; set; }

    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfiguration(new ShoppingItemConfiguration());
        modelBuilder.ApplyConfiguration(new ShoppingItemDocumentConfiguration());
        modelBuilder.AddApplicationDbContextDataSeed();
    }
}
