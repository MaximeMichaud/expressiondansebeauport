using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHelpArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HelpArticles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    ContentMode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "html"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    RouteHint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Created = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpArticles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HelpArticles_RouteHint",
                table: "HelpArticles",
                column: "RouteHint");

            migrationBuilder.CreateIndex(
                name: "IX_HelpArticles_Slug",
                table: "HelpArticles",
                column: "Slug",
                unique: true,
                filter: "\"Deleted\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HelpArticles");
        }
    }
}
