using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAutosaveUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Supprimer les autosaves en double avant de créer l'index unique
            migrationBuilder.Sql("""
                DELETE FROM "PageRevisions"
                WHERE "RevisionType" = 'Autosave'
                  AND "Id" NOT IN (
                    SELECT DISTINCT ON ("PageId") "Id"
                    FROM "PageRevisions"
                    WHERE "RevisionType" = 'Autosave'
                    ORDER BY "PageId", "CreatedAt" DESC
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_PageRevisions_PageId_Autosave_Unique",
                table: "PageRevisions",
                column: "PageId",
                unique: true,
                filter: "\"RevisionType\" = 'Autosave'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PageRevisions_PageId_Autosave_Unique",
                table: "PageRevisions");
        }
    }
}
