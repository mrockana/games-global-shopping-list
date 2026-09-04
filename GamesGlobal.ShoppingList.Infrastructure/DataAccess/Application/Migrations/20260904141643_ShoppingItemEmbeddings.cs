using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.Migrations
{
    /// <inheritdoc />
    public sealed partial class ShoppingItemEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 2L);

            migrationBuilder.AddColumn<Vector>(
                name: "Embeddings",
                table: "ShoppingItems",
                type: "vector(768)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embeddings",
                table: "ShoppingItems");

            migrationBuilder.InsertData(
                table: "ShoppingItems",
                columns: new[] { "ShoppingItemId", "Created", "Description", "Modified", "Name", "UserCode" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Blue denim jean, medium size.", null, "Pants", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 2L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), "White plain shirt, medium size.", null, "Shirt", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                });
        }
    }
}
