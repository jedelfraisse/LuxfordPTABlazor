using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddEventControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    ControlType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDirector = table.Column<bool>(type: "bit", nullable: false),
                    SettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventControls_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventControls_EventId_IsDirector",
                table: "EventControls",
                columns: new[] { "EventId", "IsDirector" },
                unique: true,
                filter: "[IsDirector] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_EventControls_EventId_SequenceOrder",
                table: "EventControls",
                columns: new[] { "EventId", "SequenceOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventControls");
        }
    }
}
