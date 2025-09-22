using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDragonName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DragonName",
                table: "SchoolYears");

            migrationBuilder.DropColumn(
                name: "DragonPersona",
                table: "SchoolYears");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DragonName",
                table: "SchoolYears",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DragonPersona",
                table: "SchoolYears",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
