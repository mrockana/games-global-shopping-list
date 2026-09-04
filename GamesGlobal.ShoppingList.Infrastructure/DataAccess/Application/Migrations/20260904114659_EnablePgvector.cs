using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.Migrations
{
    /// <inheritdoc />
    public sealed partial class EnablePgvector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
