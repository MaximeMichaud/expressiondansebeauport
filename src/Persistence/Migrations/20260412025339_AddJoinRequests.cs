using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJoinRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JoinRequestId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageType",
                table: "Messages",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Text");

            migrationBuilder.CreateTable(
                name: "JoinRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ResolvedByMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoinRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JoinRequests_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JoinRequests_Members_RequesterMemberId",
                        column: x => x.RequesterMemberId,
                        principalTable: "Members",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JoinRequests_Members_ResolvedByMemberId",
                        column: x => x.ResolvedByMemberId,
                        principalTable: "Members",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_JoinRequestId",
                table: "Messages",
                column: "JoinRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_JoinRequests_GroupId_RequesterMemberId_Status",
                table: "JoinRequests",
                columns: new[] { "GroupId", "RequesterMemberId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_JoinRequests_RequesterMemberId",
                table: "JoinRequests",
                column: "RequesterMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_JoinRequests_ResolvedByMemberId",
                table: "JoinRequests",
                column: "ResolvedByMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_JoinRequests_JoinRequestId",
                table: "Messages",
                column: "JoinRequestId",
                principalTable: "JoinRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_JoinRequests_JoinRequestId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "JoinRequests");

            migrationBuilder.DropIndex(
                name: "IX_Messages_JoinRequestId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "JoinRequestId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "Messages");
        }
    }
}
