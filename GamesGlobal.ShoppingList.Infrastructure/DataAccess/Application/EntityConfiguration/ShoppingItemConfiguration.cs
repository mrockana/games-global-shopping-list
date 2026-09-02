using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.EntityConfiguration;

internal sealed class ShoppingItemConfiguration : IEntityTypeConfiguration<ShoppingItem>
{
    public void Configure(EntityTypeBuilder<ShoppingItem> builder)
    {
        builder.HasIndex(shoppingItem => shoppingItem.UserCode);
    }
}
