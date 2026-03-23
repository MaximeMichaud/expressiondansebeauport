using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicFooter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CopyrightText",
                table: "SiteSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterAddress",
                table: "SiteSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterCity",
                table: "SiteSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterDescription",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterEmail",
                table: "SiteSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterPhone",
                table: "SiteSettings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "SiteSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FooterPartners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteSettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FooterPartners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FooterPartners_MediaFiles_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "MediaFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FooterPartners_SiteSettings_SiteSettingsId",
                        column: x => x.SiteSettingsId,
                        principalTable: "SiteSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteSettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialLinks_SiteSettings_SiteSettingsId",
                        column: x => x.SiteSettingsId,
                        principalTable: "SiteSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FooterPartners_MediaFileId",
                table: "FooterPartners",
                column: "MediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_FooterPartners_SiteSettingsId",
                table: "FooterPartners",
                column: "SiteSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialLinks_SiteSettingsId",
                table: "SocialLinks",
                column: "SiteSettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FooterPartners");

            migrationBuilder.DropTable(
                name: "SocialLinks");

            migrationBuilder.DropColumn(
                name: "CopyrightText",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FooterAddress",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FooterCity",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FooterDescription",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FooterEmail",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "FooterPhone",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "SiteSettings");
        }
    }
}
