using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.Infrastructure.DataAccess.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Identity.EntityConfiguration;

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasMany(u => u.Roles)
       .WithMany(r => r.Users)
       .UsingEntity<UserRole>(
       j =>
       {
           j.ToTable("UserRoles", DataAccessConstants.IdentitySchema);
           j.HasKey(ur => new { ur.UserId, ur.RoleId });
       });
    }
}