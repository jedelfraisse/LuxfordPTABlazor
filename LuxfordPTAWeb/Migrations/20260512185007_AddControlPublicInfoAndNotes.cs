using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddControlPublicInfoAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NotesHtml",
                table: "EventControls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NotesMarkdown",
                table: "EventControls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "PublicInformation",
                table: "EventControls",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotesHtml",
                table: "EventControls");

            migrationBuilder.DropColumn(
                name: "NotesMarkdown",
                table: "EventControls");

            migrationBuilder.DropColumn(
                name: "PublicInformation",
                table: "EventControls");
        }
    }
}
