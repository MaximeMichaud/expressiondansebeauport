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
