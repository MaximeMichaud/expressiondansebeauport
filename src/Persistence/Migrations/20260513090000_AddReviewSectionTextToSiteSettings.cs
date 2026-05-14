using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GarneauTemplateDbContext))]
    [Migration("20260513090000_AddReviewSectionTextToSiteSettings")]
    public partial class AddReviewSectionTextToSiteSettings : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
