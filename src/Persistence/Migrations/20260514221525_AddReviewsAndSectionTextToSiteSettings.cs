using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewsAndSectionTextToSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewsSectionEyebrow",
                table: "SiteSettings",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Avis de notre communauté");

            migrationBuilder.AddColumn<string>(
                name: "ReviewsSectionSubtitle",
                table: "SiteSettings",
                type: "character varying(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "Quelques témoignages gérés par l’équipe du site pour refléter l’expérience vécue à l’école.");

            migrationBuilder.AddColumn<string>(
                name: "ReviewsSectionTitle",
                table: "SiteSettings",
                type: "character varying(140)",
                maxLength: 140,
                nullable: false,
                defaultValue: "Ce que les familles disent de nous");

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteSettingsId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Comment = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    Author = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_SiteSettings_SiteSettingsId",
                        column: x => x.SiteSettingsId,
                        principalTable: "SiteSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_SiteSettingsId",
                table: "Reviews",
                column: "SiteSettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropColumn(
                name: "ReviewsSectionEyebrow",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ReviewsSectionSubtitle",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "ReviewsSectionTitle",
                table: "SiteSettings");
        }
    }
}
