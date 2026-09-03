using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.Infrastructure.DataAccess.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.EntityConfiguration;

internal sealed class ShoppingItemDocumentConfiguration : IEntityTypeConfiguration<ShoppingItem>
{
    public void Configure(EntityTypeBuilder<ShoppingItem> builder)
    {
        builder.HasMany(s => s.Documents)
       .WithMany(d => d.ShoppingItems)
    .UsingEntity<ShoppingItemDocument>(
    j => j
        .HasOne(join => join.Document)
        .WithMany()
        .HasForeignKey(join => join.DocumentId),
    j => j
        .HasOne(join => join.ShoppingItem)
        .WithMany()
        .HasForeignKey(join => join.ShoppingItemId),
    j =>
    {
        j.ToTable("ShoppingItemDocuments", DataAccessConstants.ApplicationSchema);
        j.HasKey(join => new { join.DocumentId, join.ShoppingItemId });
    });
    }
}