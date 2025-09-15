using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceEventModelPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CleanupEndTime",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedAttendees",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventCoordinatorId",
                table: "Events",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EventEndTime",
                table: "Events",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EventStartTime",
                table: "Events",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ExcelImportId",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxAttendees",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublicInstructions",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCleanup",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresSetup",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresVolunteers",
                table: "Events",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SetupStartTime",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WeatherBackupPlan",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventCoordinatorId",
                table: "Events",
                column: "EventCoordinatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_AspNetUsers_EventCoordinatorId",
                table: "Events",
                column: "EventCoordinatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_AspNetUsers_EventCoordinatorId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_EventCoordinatorId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CleanupEndTime",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EstimatedAttendees",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventCoordinatorId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventEndTime",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventStartTime",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ExcelImportId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MaxAttendees",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "PublicInstructions",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RequiresCleanup",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RequiresSetup",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RequiresVolunteers",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SetupStartTime",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "WeatherBackupPlan",
                table: "Events");
        }
    }
}
