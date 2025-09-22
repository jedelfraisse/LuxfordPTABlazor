using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardElections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ElectionEventId",
                table: "BoardPositionTitles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsElected",
                table: "BoardPositionTitles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElectionEventId",
                table: "BoardPositionTitles");

            migrationBuilder.DropColumn(
                name: "IsElected",
                table: "BoardPositionTitles");
        }
    }
}
