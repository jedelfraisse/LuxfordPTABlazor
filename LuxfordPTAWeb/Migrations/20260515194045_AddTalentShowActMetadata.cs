using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddTalentShowActMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HostIntro",
                table: "TalentShowActs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LightingNotes",
                table: "TalentShowActs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MusicStartOffsetSeconds",
                table: "TalentShowActs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MusicUrl",
                table: "TalentShowActs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PerformerNotes",
                table: "TalentShowActs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PropsRequired",
                table: "TalentShowActs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SoundNotes",
                table: "TalentShowActs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StageNotes",
                table: "TalentShowActs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HostIntro",
                table: "TalentShowActs");

            migrationBuilder.DropColumn(
                name: "LightingNotes",
                table: "TalentShowActs");

            migrationBuilder.DropColumn(
                name: "MusicStartOffsetSeconds",
                table: "TalentShowActs");

            migrationBuilder.DropColumn(
                name: "MusicUrl",
                table: "TalentShowActs");

            migrationBuilder.DropColumn(
                name: "PerformerNotes",
                table: "TalentShowActs");

            migrationBuilder.DropColumn(
                name: "PropsRequired",
                table: "TalentShowActs");

            migrationBuilder.DropColumn(
                name: "SoundNotes",
                table: "TalentShowActs");

            migrationBuilder.DropColumn(
                name: "StageNotes",
                table: "TalentShowActs");
        }
    }
}
