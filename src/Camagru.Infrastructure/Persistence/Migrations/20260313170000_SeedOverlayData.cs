using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Camagru.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260313170000_SeedOverlayData")]
    public partial class SeedOverlayData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "Overlays" ("Name", "Category", "FilePath", "DisplayOrder", "CreatedAt")
                VALUES
                    ('Hearts', 'Hearts', '/overlays/hearts.png', 0, CURRENT_TIMESTAMP),
                    ('Stars', 'Stars', '/overlays/stars.png', 1, CURRENT_TIMESTAMP),
                    ('Emoji', 'Emoji', '/overlays/emoji.png', 2, CURRENT_TIMESTAMP);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "Overlays"
                WHERE "Name" IN ('Hearts', 'Stars', 'Emoji');
                """);
        }
    }
}
