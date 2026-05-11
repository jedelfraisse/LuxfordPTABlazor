using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTemplateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EventCatId = table.Column<int>(type: "int", nullable: false),
                    EventSubTypeId = table.Column<int>(type: "int", nullable: true),
                    DefaultTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultDescriptionMarkdown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultMoreDetailsMarkdown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiresVolunteers = table.Column<bool>(type: "bit", nullable: false),
                    RequiresSetup = table.Column<bool>(type: "bit", nullable: false),
                    RequiresCleanup = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTemplates_EventCatSubs_EventSubTypeId",
                        column: x => x.EventSubTypeId,
                        principalTable: "EventCatSubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EventTemplates_EventCats_EventCatId",
                        column: x => x.EventCatId,
                        principalTable: "EventCats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplates_EventCatId",
                table: "EventTemplates",
                column: "EventCatId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplates_EventSubTypeId",
                table: "EventTemplates",
                column: "EventSubTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplates_IsActive_EventCatId",
                table: "EventTemplates",
                columns: new[] { "IsActive", "EventCatId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventTemplates");
        }
    }
}
