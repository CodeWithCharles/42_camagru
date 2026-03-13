using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Camagru.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedOverlayData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Overlays",
                columns: new[] { "Name", "Category", "FilePath", "DisplayOrder", "CreatedAt" },
                values: new object[,]
                {
                    { "Hearts", "Hearts", "/overlays/hearts.png", 0, DateTime.UtcNow },
                    { "Stars", "Stars", "/overlays/stars.png", 1, DateTime.UtcNow },
                    { "Emoji", "Emoji", "/overlays/emoji.png", 2, DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Overlays",
                keyColumn: "Name",
                keyValue: "Hearts");

            migrationBuilder.DeleteData(
                table: "Overlays",
                keyColumn: "Name",
                keyValue: "Stars");

            migrationBuilder.DeleteData(
                table: "Overlays",
                keyColumn: "Name",
                keyValue: "Emoji");
        }
    }
}
