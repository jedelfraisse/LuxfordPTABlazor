using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiDayEventSupportV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_SchoolYearId",
                table: "Events");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "Events",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CopyGeneration",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsMultiDay",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Events",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SourceEventId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    DayNumber = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxAttendees = table.Column<int>(type: "int", nullable: true),
                    EstimatedAttendees = table.Column<int>(type: "int", nullable: true),
                    WeatherBackupPlan = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventDays_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_ApprovedByUserId",
                table: "Events",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_SchoolYearId_Status",
                table: "Events",
                columns: new[] { "SchoolYearId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_Slug",
                table: "Events",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_Events_SourceEventId",
                table: "Events",
                column: "SourceEventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventDays_EventId_DayNumber",
                table: "EventDays",
                columns: new[] { "EventId", "DayNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_ApprovedBy",
                table: "Events",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_SourceEvent",
                table: "Events",
                column: "SourceEventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_ApprovedBy",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_SourceEvent",
                table: "Events");

            migrationBuilder.DropTable(
                name: "EventDays");

            migrationBuilder.DropIndex(
                name: "IX_Events_ApprovedByUserId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_SchoolYearId_Status",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_Slug",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_SourceEventId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CopyGeneration",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsMultiDay",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SourceEventId",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_Events_SchoolYearId",
                table: "Events",
                column: "SchoolYearId");
        }
    }
}
