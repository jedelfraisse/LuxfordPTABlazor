using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class EventCatFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventCatSubs_EventCats_EventTypeId",
                table: "EventCatSubs");

            migrationBuilder.RenameColumn(
                name: "EventTypeId",
                table: "EventCatSubs",
                newName: "EventCatId");

            migrationBuilder.RenameIndex(
                name: "IX_EventCatSubs_EventTypeId",
                table: "EventCatSubs",
                newName: "IX_EventCatSubs_EventCatId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventCatSubs_EventCats_EventCatId",
                table: "EventCatSubs",
                column: "EventCatId",
                principalTable: "EventCats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventCatSubs_EventCats_EventCatId",
                table: "EventCatSubs");

            migrationBuilder.RenameColumn(
                name: "EventCatId",
                table: "EventCatSubs",
                newName: "EventTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_EventCatSubs_EventCatId",
                table: "EventCatSubs",
                newName: "IX_EventCatSubs_EventTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventCatSubs_EventCats_EventTypeId",
                table: "EventCatSubs",
                column: "EventTypeId",
                principalTable: "EventCats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
