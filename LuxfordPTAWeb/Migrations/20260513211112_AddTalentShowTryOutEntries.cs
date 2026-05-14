using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddTalentShowTryOutEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TalentShowTryOutEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    SignupId = table.Column<int>(type: "int", nullable: true),
                    ActId = table.Column<int>(type: "int", nullable: true),
                    PerformerNames = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    ActTitle = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SlotTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Selected = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalentShowTryOutEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentShowTryOutEntries_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TalentShowTryOutEntries_TalentShowSignups_SignupId",
                        column: x => x.SignupId,
                        principalTable: "TalentShowSignups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TalentShowTryOutEntries_EventId_SlotTime",
                table: "TalentShowTryOutEntries",
                columns: new[] { "EventId", "SlotTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TalentShowTryOutEntries_SignupId",
                table: "TalentShowTryOutEntries",
                column: "SignupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TalentShowTryOutEntries");
        }
    }
}
