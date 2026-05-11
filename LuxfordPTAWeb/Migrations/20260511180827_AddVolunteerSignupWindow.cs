using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddVolunteerSignupWindow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SignupWindowEnd",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SignupWindowStart",
                table: "Events",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignupWindowEnd",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SignupWindowStart",
                table: "Events");
        }
    }
}
