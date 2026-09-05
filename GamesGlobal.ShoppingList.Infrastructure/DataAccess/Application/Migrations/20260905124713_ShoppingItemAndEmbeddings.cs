using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.Migrations
{
    /// <inheritdoc />
    public sealed partial class ShoppingItemAndEmbeddings : Migration
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

            migrationBuilder.InsertData(
                table: "ShoppingItems",
                columns: new[] { "ShoppingItemId", "Created", "Description", "Embeddings", "Modified", "Name", "UserCode" },
                values: new object[,]
                {
                    { 5L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Essence Mascara Lash Princess is a popular mascara known for its volumizing and lengthening effects. Achieve dramatic lashes with this long-lasting and cruelty-free formula.", null, null, "Essence Mascara Lash Princess", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 6L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Eyeshadow Palette with Mirror offers a versatile range of eyeshadow shades for creating stunning eye looks. With a built-in mirror, it is convenient for on-the-go makeup application.", null, null, "Eyeshadow Palette with Mirror", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 7L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Powder Canister is a finely milled setting powder designed to set makeup and control shine. With a lightweight and translucent formula, it provides a smooth and matte finish.", null, null, "Powder Canister", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 8L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Red Lipstick is a classic and bold choice for adding a pop of color to your lips. With a creamy and pigmented formula, it provides a vibrant and long-lasting finish.", null, null, "Red Lipstick", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 9L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Red Nail Polish offers a rich and glossy red hue for vibrant and polished nails. With a quick-drying formula, it provides a salon-quality finish at home.", null, null, "Red Nail Polish", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 10L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "CK One by Calvin Klein is a classic unisex fragrance, known for its fresh and clean scent. It is a versatile fragrance suitable for everyday wear.", null, null, "Calvin Klein CK One", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 11L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Coco Noir by Chanel is an elegant and mysterious fragrance, featuring notes of grapefruit, rose, and sandalwood. Perfect for evening occasions.", null, null, "Chanel Coco Noir Eau De", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 12L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "J'adore by Dior is a luxurious and floral fragrance, known for its blend of ylang-ylang, rose, and jasmine. It embodies femininity and sophistication.", null, null, "Dior J'adore", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 13L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Dolce Shine by Dolce and Gabbana is a vibrant and fruity fragrance, featuring notes of mango, jasmine, and blonde woods. It is a joyful and youthful scent.", null, null, "Dolce Shine Eau de", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 14L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Gucci Bloom by Gucci is a floral and captivating fragrance, with notes of tuberose, jasmine, and Rangoon creeper. It is a modern and romantic scent.", null, null, "Gucci Bloom Eau de", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 35L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Essence Mascara Lash Princess is a popular mascara known for its volumizing and lengthening effects. Achieve dramatic lashes with this long-lasting and cruelty-free formula.", null, null, "Essence Mascara Lash Princess", new Guid("00c2f66a-e396-4865-9338-d9cb8faff7d4") },
                    { 36L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Eyeshadow Palette with Mirror offers a versatile range of eyeshadow shades for creating stunning eye looks. With a built-in mirror, it is convenient for on-the-go makeup application.", null, null, "Eyeshadow Palette with Mirror", new Guid("00c2f66a-e396-4865-9338-d9cb8faff7d4") },
                    { 37L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Powder Canister is a finely milled setting powder designed to set makeup and control shine. With a lightweight and translucent formula, it provides a smooth and matte finish.", null, null, "Powder Canister", new Guid("00c2f66a-e396-4865-9338-d9cb8faff7d4") },
                    { 38L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Red Lipstick is a classic and bold choice for adding a pop of color to your lips. With a creamy and pigmented formula, it provides a vibrant and long-lasting finish.", null, null, "Red Lipstick", new Guid("00c2f66a-e396-4865-9338-d9cb8faff7d4") },
                    { 39L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Red Nail Polish offers a rich and glossy red hue for vibrant and polished nails. With a quick-drying formula, it provides a salon-quality finish at home.", null, null, "Red Nail Polish", new Guid("00c2f66a-e396-4865-9338-d9cb8faff7d4") },
                    { 40L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "CK One by Calvin Klein is a classic unisex fragrance, known for its fresh and clean scent. It is a versatile fragrance suitable for everyday wear.", null, null, "Calvin Klein CK One", new Guid("00c2f66a-e396-4865-9338-d9cb8faff7d4") },
                    { 78L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Knoll Saarinen Executive Conference Chair is a modern and ergonomic chair, perfect for your office or conference room with its timeless design.", null, null, "Knoll Saarinen Executive Conference Chair", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 79L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "The Wooden Bathroom Sink with Mirror is a unique and stylish addition to your bathroom, featuring a wooden sink countertop and a matching mirror.", null, null, "Wooden Bathroom Sink With Mirror", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 80L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Fresh and crisp apples, perfect for snacking or incorporating into various recipes.", null, null, "Apple", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 81L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "High-quality beef steak, great for grilling or cooking to your preferred level of doneness.", null, null, "Beef Steak", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 82L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Nutritious cat food formulated to meet the dietary needs of your feline friend.", null, null, "Cat Food", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 83L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Fresh and tender chicken meat, suitable for various culinary preparations.", null, null, "Chicken Meat", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 84L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Versatile cooking oil suitable for frying, sauteing, and various culinary applications.", null, null, "Cooking Oil", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 85L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Crisp and hydrating cucumbers, ideal for salads, snacks, or as a refreshing side.", null, null, "Cucumber", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 86L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Specially formulated dog food designed to provide essential nutrients for your canine companion.", null, null, "Dog Food", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 87L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Fresh eggs, a versatile ingredient for baking, cooking, or breakfast.", null, null, "Eggs", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 88L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Quality fish steak, suitable for grilling, baking, or pan-searing.", null, null, "Fish Steak", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 89L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Fresh and vibrant green bell pepper, perfect for adding color and flavor to your dishes.", null, null, "Green Bell Pepper", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 90L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Spicy green chili pepper, ideal for adding heat to your favorite recipes.", null, null, "Green Chili Pepper", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 91L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Pure and natural honey in a convenient jar, perfect for sweetening beverages or drizzling over food.", null, null, "Honey Jar", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 92L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Creamy and delicious ice cream, available in various flavors for a delightful treat.", null, null, "Ice Cream", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 93L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Refreshing fruit juice, packed with vitamins and great for staying hydrated.", null, null, "Juice", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                    { 94L, new DateTime(2025, 4, 30, 9, 41, 2, 53, DateTimeKind.Utc), "Nutrient-rich kiwi, perfect for snacking or adding a tropical twist to your dishes.", null, null, "Kiwi", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 35L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 36L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 37L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 38L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 39L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 40L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 78L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 79L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 80L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 81L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 82L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 83L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 84L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 85L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 86L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 87L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 88L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 89L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 90L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 91L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 92L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 93L);

            migrationBuilder.DeleteData(
                table: "ShoppingItems",
                keyColumn: "ShoppingItemId",
                keyValue: 94L);

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
