using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceEventTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorClass",
                table: "EventTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "EventTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "EventTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EventTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "EventTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ColorClass", "Description", "DisplayOrder", "Icon", "IsActive", "Size" },
                values: new object[] { "text-danger", "All holidays, staff days, and special schedule days for the school year.", 1, "bi-calendar-x", true, 1 });

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ColorClass", "Description", "DisplayOrder", "Icon", "IsActive", "Size" },
                values: new object[] { "text-success", "Events focused on raising funds to support school programs, equipment, and activities.", 2, "bi-piggy-bank", true, 0 });

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ColorClass", "Description", "DisplayOrder", "Icon", "IsActive", "Size" },
                values: new object[] { "text-primary", "Events designed to build community, support families, and create fun experiences for students.", 3, "bi-people", true, 0 });

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ColorClass", "Description", "DisplayOrder", "Icon", "IsActive", "Size" },
                values: new object[] { "text-info", "Events focused on supporting our educators, staff, and school operations.", 4, "bi-mortarboard", true, 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorClass",
                table: "EventTypes");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "EventTypes");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "EventTypes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EventTypes");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "EventTypes");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "");
        }
    }
}
