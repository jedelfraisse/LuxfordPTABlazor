using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddEventColorOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventColorOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CssClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventColorOptions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "EventColorOptions",
                columns: new[] { "Id", "CssClass", "DisplayOrder", "Name" },
                values: new object[,]
                {
                    { 1, "text-primary", 1, "Blue" },
                    { 2, "text-success", 2, "Green" },
                    { 3, "text-danger", 3, "Red" },
                    { 4, "text-warning", 4, "Yellow" },
                    { 5, "text-info", 5, "Cyan" },
                    { 6, "text-secondary", 6, "Gray" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventColorOptions");
        }
    }
}
