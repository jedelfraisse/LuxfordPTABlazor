using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsMandatoryFromEventTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMandatory",
                table: "EventTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMandatory",
                table: "EventTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
