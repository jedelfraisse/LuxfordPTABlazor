using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    public partial class AddEventFlyerUrls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FlyerUrlsJson",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlyerUrlsJson",
                table: "Events");
        }
    }
}
