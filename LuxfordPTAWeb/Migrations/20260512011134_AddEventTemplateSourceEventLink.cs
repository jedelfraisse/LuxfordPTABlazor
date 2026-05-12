using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTemplateSourceEventLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceEventId",
                table: "EventTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTemplates_SourceEventId",
                table: "EventTemplates",
                column: "SourceEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventTemplates_Events_SourceEventId",
                table: "EventTemplates",
                column: "SourceEventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventTemplates_Events_SourceEventId",
                table: "EventTemplates");

            migrationBuilder.DropIndex(
                name: "IX_EventTemplates_SourceEventId",
                table: "EventTemplates");

            migrationBuilder.DropColumn(
                name: "SourceEventId",
                table: "EventTemplates");
        }
    }
}
