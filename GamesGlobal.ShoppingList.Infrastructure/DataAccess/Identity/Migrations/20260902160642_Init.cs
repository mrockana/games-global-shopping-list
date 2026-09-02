using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Identity.Migrations
{
    /// <inheritdoc />
    public sealed partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "identity",
                columns: table => new
                {
                    RoleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserCode = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "identity",
                columns: table => new
                {
                    RolePermissionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Permission = table.Column<long>(type: "bigint", nullable: false),
                    PermissionName = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.RolePermissionId);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "identity",
                columns: table => new
                {
                    LoginSessionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.LoginSessionId);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Roles",
                columns: new[] { "RoleId", "Created", "Modified", "Name" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), null, "Super Admin" },
                    { 2L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), null, "End User" },
                    { 3L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), null, "Auditor" },
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Users",
                columns: new[] { "UserId", "Created", "Email", "FirstName", "LastName", "Modified", "Password", "UserCode" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), "johndoe@example.gamesglobal", "Test", "Shopper", null, "AQAAAAIAAYagAAAAEMjOMRX3KUP+HAnuNXdSUSKWXkho+10WudwJqneRzr8rgcd4twupbzUOSvho8I5qgA==", new Guid("d3b07384-d9a0-4f1e-8c2e-1f2b3c4d5e6f") },
                    { 2L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), "admin@example.gamesglobal", "Super", "Admin", null, "AQAAAAIAAYagAAAAEMjOMRX3KUP+HAnuNXdSUSKWXkho+10WudwJqneRzr8rgcd4twupbzUOSvho8I5qgA==", new Guid("00c2f66a-e396-4865-9338-d9cb8faff7d4") },
                    { 3L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), "auditor@example.gamesglobal", "Auditor", "General", null, "AQAAAAIAAYagAAAAEMjOMRX3KUP+HAnuNXdSUSKWXkho+10WudwJqneRzr8rgcd4twupbzUOSvho8I5qgA==", new Guid("54e66112-3ef2-4ed5-8fb7-d5ca167f07b1") },
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "RolePermissionId", "Created", "Modified", "Permission", "PermissionName", "RoleId" },
                values: new object[,]
                {
                    { 1L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), null, -1L, "All", 1L },
                    { 2L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), null, 1L, "Shopping Items Self Read/Write", 2L },
                    { 3L, new DateTime(2025, 4, 1, 12, 0, 0, 0, DateTimeKind.Utc), null, 4L, "User Roles And Permissions Read", 3L },
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { 2L, 1L },
                    { 1L, 2L },
                    { 3L, 3L },
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_Token",
                schema: "identity",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "identity",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                schema: "identity",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "identity",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_User_UserCode",
                schema: "identity",
                table: "Users",
                column: "UserCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "identity",
                table: "Users",
                column: "Email",
                unique: true);

            // Keeps FindRoleByName's LOWER("Name") comparison sargable; PostgreSQL is case-sensitive by default.
            migrationBuilder.Sql(
                "CREATE INDEX \"IX_Roles_Name_Lower\" ON identity.\"Roles\" (LOWER(\"Name\"));");

            // Seeded rows supply explicit keys, which does not advance the PostgreSQL identity sequences.
            migrationBuilder.Sql(
                "SELECT setval(pg_get_serial_sequence('identity.\"Users\"', 'UserId'), (SELECT MAX(\"UserId\") FROM identity.\"Users\"));");
            migrationBuilder.Sql(
                "SELECT setval(pg_get_serial_sequence('identity.\"Roles\"', 'RoleId'), (SELECT MAX(\"RoleId\") FROM identity.\"Roles\"));");
            migrationBuilder.Sql(
                "SELECT setval(pg_get_serial_sequence('identity.\"RolePermissions\"', 'RolePermissionId'), (SELECT MAX(\"RolePermissionId\") FROM identity.\"RolePermissions\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "identity");
        }
    }
}
