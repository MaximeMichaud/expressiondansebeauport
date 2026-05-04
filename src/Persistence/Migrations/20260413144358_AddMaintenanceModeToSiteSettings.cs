using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceModeToSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMaintenanceMode",
                table: "SiteSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceMessage",
                table: "SiteSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "Le site est en maintenance. Revenez bientôt !");

            migrationBuilder.AddColumn<int>(
                name: "MaintenanceRetryAfter",
                table: "SiteSettings",
                type: "integer",
                nullable: false,
                defaultValue: 3600);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMaintenanceMode",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "MaintenanceMessage",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "MaintenanceRetryAfter",
                table: "SiteSettings");
        }
    }
}
