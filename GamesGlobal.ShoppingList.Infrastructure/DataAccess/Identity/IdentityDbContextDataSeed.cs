using System;
using GamesGlobal.ShoppingList.BusinessDomain.Common.EnumHelper;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Identity;

internal static class IdentityDbContextDataSeed
{
    internal static void AddIdentityDbContextDataSeed(this ModelBuilder modelBuilder)
    {
        // TODO: > These <User> data records are intended for local development only; they will be removed in production. Please use the appropriate <User> data records on higher environments.
        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = 1L,
            FirstName = "Test",
            LastName = "Shopper",
            UserCode = Guid.Parse("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f"),
            Email = "johndoe@example.gamesglobal",
            Password = "AQAAAAIAAYagAAAAEMjOMRX3KUP+HAnuNXdSUSKWXkho+10WudwJqneRzr8rgcd4twupbzUOSvho8I5qgA==", // 123Abc123@
            Created = new DateTime(2025, 04, 1, 12, 0, 0, DateTimeKind.Utc),
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = 2L,
            FirstName = "Super",
            LastName = "Admin",
            Email = "admin@example.gamesglobal",
            UserCode = Guid.Parse("00c2f66a-e396-4865-9338-d9cb8faff7d4"),
            Password = "AQAAAAIAAYagAAAAEMjOMRX3KUP+HAnuNXdSUSKWXkho+10WudwJqneRzr8rgcd4twupbzUOSvho8I5qgA==", // 123Abc123@
            Created = new DateTime(2025, 04, 1, 12, 0, 0, DateTimeKind.Utc),
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = 3L,
            FirstName = "Auditor",
            LastName = "General",
            Email = "auditor@example.gamesglobal",
            UserCode = Guid.Parse("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1"),
            Password = "AQAAAAIAAYagAAAAEMjOMRX3KUP+HAnuNXdSUSKWXkho+10WudwJqneRzr8rgcd4twupbzUOSvho8I5qgA==", // 123Abc123@
            Created = new DateTime(2025, 04, 1, 12, 0, 0, DateTimeKind.Utc),
        });

        modelBuilder.Entity<Role>().HasData(new Role
        {
            RoleId = 1L,
            Name = "Super Admin",
            Created = new DateTime(2025, 04, 1, 12, 0, 0, DateTimeKind.Utc),
        });

        modelBuilder.Entity<Role>().HasData(new Role
        {
            RoleId = 2L,
            Name = "End User",
            Created = new DateTime(2025, 04, 1, 12, 0, 0, DateTimeKind.Utc),
        });

        modelBuilder.Entity<Role>().HasData(new Role
        {
            RoleId = 3L,
            Name = "Auditor",
            Created = new DateTime(2025, 04, 1, 12, 0, 0, DateTimeKind.Utc),
        });

        modelBuilder.Entity<RolePermission>().HasData(new RolePermission
        {
            RolePermissionId = 1L,
            Permission = Permissions.All,
            PermissionName = Permissions.All.GetDescription(),
            RoleId = 1L, // Super Admin
            Created = new DateTime(2025, 04, 1, 12, 0, 0, DateTimeKind.Utc),
        });

        modelBuilder.Entity<RolePermission>().HasData(new RolePermission
        {
            RolePermissionId = 2L,
            Permission = Permissions.ShoppingItemsSelfReadWrite,
            PermissionName = Permissions.ShoppingItemsSelfReadWrite.GetDescription(),
            RoleId = 2L, // End User
            Created = new DateTime(2025, 04, 1, 12, 0, 0, DateTimeKind.Utc),
        });

        modelBuilder.Entity<RolePermission>().HasData(new RolePermission
        {
            RolePermissionId = 3L,
            Permission = Permissions.UserRolesAndPermissionsReadOnly,
            PermissionName = Permissions.UserRolesAndPermissionsReadOnly.GetDescription(),
            RoleId = 3L, // Auditor
            Created = new DateTime(2025, 04, 1, 12, 0, 0, DateTimeKind.Utc),
        });

        modelBuilder.Entity<UserRole>().HasData(new UserRole
        {
            UserId = 1L, // Test Founder
            RoleId = 2L, // End User
        });

        modelBuilder.Entity<UserRole>().HasData(new UserRole
        {
            UserId = 2L, // Super Admin
            RoleId = 1L, // Super Admin
        });

        modelBuilder.Entity<UserRole>().HasData(new UserRole
        {
            UserId = 3L, // Auditor General
            RoleId = 3L, // Auditor
        });
    }
}