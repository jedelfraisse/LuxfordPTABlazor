using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddTryOutsRefinementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInTimestamp",
                table: "TalentShowTryOutEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JudgeCompletionsJson",
                table: "TalentShowTryOutEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MusicUrl",
                table: "TalentShowTryOutEntries",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PickupAdult",
                table: "TalentShowTryOutEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ScoresJson",
                table: "TalentShowTryOutEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MusicUrl",
                table: "TalentShowSignups",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PickupAdult",
                table: "TalentShowSignups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInTimestamp",
                table: "TalentShowTryOutEntries");

            migrationBuilder.DropColumn(
                name: "JudgeCompletionsJson",
                table: "TalentShowTryOutEntries");

            migrationBuilder.DropColumn(
                name: "MusicUrl",
                table: "TalentShowTryOutEntries");

            migrationBuilder.DropColumn(
                name: "PickupAdult",
                table: "TalentShowTryOutEntries");

            migrationBuilder.DropColumn(
                name: "ScoresJson",
                table: "TalentShowTryOutEntries");

            migrationBuilder.DropColumn(
                name: "MusicUrl",
                table: "TalentShowSignups");

            migrationBuilder.DropColumn(
                name: "PickupAdult",
                table: "TalentShowSignups");
        }
    }
}
