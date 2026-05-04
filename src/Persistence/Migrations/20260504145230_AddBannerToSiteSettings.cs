using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerToSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannerText",
                table: "SiteSettings",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "Actualités du moment");

            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                table: "SiteSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "/actualites");

            migrationBuilder.AddColumn<bool>(
                name: "IsBannerEnabled",
                table: "SiteSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerText",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "BannerUrl",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "IsBannerEnabled",
                table: "SiteSettings");
        }
    }
}
