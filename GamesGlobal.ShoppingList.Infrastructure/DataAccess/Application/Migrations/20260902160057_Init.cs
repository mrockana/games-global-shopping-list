using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.Migrations
{
    /// <inheritdoc />
    public sealed partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShoppingItems",
                columns: table => new
                {
                    ShoppingItemId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserCode = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingItems", x => x.ShoppingItemId);
                });

            migrationBuilder.InsertData(
                table: "ShoppingItems",
                columns: new[] { "ShoppingItemId", "Created", "Description", "Modified", "Name", "UserCode" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Blue denim jean, medium size.", null, "Pants", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 2L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), "White plain shirt, medium size.", null, "Shirt", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingItems_UserCode",
                table: "ShoppingItems",
                column: "UserCode");

            // Seeded rows supply explicit keys, which does not advance the PostgreSQL identity sequence.
            migrationBuilder.Sql(
                "SELECT setval(pg_get_serial_sequence('\"ShoppingItems\"', 'ShoppingItemId'), (SELECT MAX(\"ShoppingItemId\") FROM \"ShoppingItems\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingItems");
        }
    }
}
