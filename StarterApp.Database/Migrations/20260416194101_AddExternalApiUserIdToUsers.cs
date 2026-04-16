using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarterApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalApiUserIdToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExternalApiUserId",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_ExternalApiUserId",
                table: "users",
                column: "ExternalApiUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_ExternalApiUserId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ExternalApiUserId",
                table: "users");
        }
    }
}
