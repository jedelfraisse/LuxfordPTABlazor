using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxfordPTAWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddTalentShowPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TalentShowActs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    PerformerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    IntroLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutroLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediaFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelectedForShow = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalentShowActs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentShowActs_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TalentShowDisplayAssignmentHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    DisplayCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnassignedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalentShowDisplayAssignmentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentShowDisplayAssignmentHistories_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TalentShowSessionStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    SessionCode = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    ShowName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OverallState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LiveSubState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextLiveSubState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentActId = table.Column<int>(type: "int", nullable: true),
                    NextActId = table.Column<int>(type: "int", nullable: true),
                    CurrentIndex = table.Column<int>(type: "int", nullable: false),
                    HostScriptPointer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntermissionEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VotingOpen = table.Column<bool>(type: "bit", nullable: false),
                    SegmentStartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedSegmentMinutes = table.Column<int>(type: "int", nullable: false),
                    IsLiveMode = table.Column<bool>(type: "bit", nullable: false),
                    PresentationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalentShowSessionStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentShowSessionStates_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TalentShowVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    ActId = table.Column<int>(type: "int", nullable: true),
                    DeviceOrUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VoterName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsJudge = table.Column<bool>(type: "bit", nullable: false),
                    TalentScore = table.Column<int>(type: "int", nullable: false),
                    StagePresenceScore = table.Column<int>(type: "int", nullable: false),
                    CreativityScore = table.Column<int>(type: "int", nullable: false),
                    CrowdEngagementScore = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalentShowVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentShowVotes_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TalentShowVotes_TalentShowActs_ActId",
                        column: x => x.ActId,
                        principalTable: "TalentShowActs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TalentShowActs_EventId_OrderIndex",
                table: "TalentShowActs",
                columns: new[] { "EventId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TalentShowDisplayAssignmentHistories_EventId_AssignedAt",
                table: "TalentShowDisplayAssignmentHistories",
                columns: new[] { "EventId", "AssignedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TalentShowSessionStates_EventId",
                table: "TalentShowSessionStates",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TalentShowSessionStates_SessionCode",
                table: "TalentShowSessionStates",
                column: "SessionCode");

            migrationBuilder.CreateIndex(
                name: "IX_TalentShowVotes_ActId",
                table: "TalentShowVotes",
                column: "ActId");

            migrationBuilder.CreateIndex(
                name: "IX_TalentShowVotes_EventId_TimestampUtc",
                table: "TalentShowVotes",
                columns: new[] { "EventId", "TimestampUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TalentShowDisplayAssignmentHistories");

            migrationBuilder.DropTable(
                name: "TalentShowSessionStates");

            migrationBuilder.DropTable(
                name: "TalentShowVotes");

            migrationBuilder.DropTable(
                name: "TalentShowActs");
        }
    }
}
