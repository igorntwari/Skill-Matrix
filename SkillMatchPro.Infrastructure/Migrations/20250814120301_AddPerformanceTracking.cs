using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatchPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeProjectPerformances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TasksAssigned = table.Column<int>(type: "integer", nullable: false),
                    TasksCompleted = table.Column<int>(type: "integer", nullable: false),
                    TasksDeliveredOnTime = table.Column<int>(type: "integer", nullable: false),
                    BugsReported = table.Column<int>(type: "integer", nullable: false),
                    CodeReviewIssues = table.Column<int>(type: "integer", nullable: false),
                    QualityScore = table.Column<decimal>(type: "numeric", nullable: false),
                    EstimatedHours = table.Column<int>(type: "integer", nullable: false),
                    ActualHours = table.Column<int>(type: "integer", nullable: false),
                    ManagerRating = table.Column<int>(type: "integer", nullable: false),
                    ManagerComments = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeProjectPerformances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeProjectPerformances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeProjectPerformances_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamCollaborations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Employee1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Employee2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollaborationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommunicationRating = table.Column<int>(type: "integer", nullable: false),
                    ConflictsResolved = table.Column<int>(type: "integer", nullable: false),
                    ConflictsEscalated = table.Column<int>(type: "integer", nullable: false),
                    WouldWorkTogetherAgain = table.Column<bool>(type: "boolean", nullable: false),
                    CollaborationScore = table.Column<decimal>(type: "numeric", nullable: false),
                    Employee1Feedback = table.Column<string>(type: "text", nullable: true),
                    Employee2Feedback = table.Column<string>(type: "text", nullable: true),
                    ManagerObservations = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamCollaborations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamCollaborations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProjectPerformances_EmployeeId_ProjectId",
                table: "EmployeeProjectPerformances",
                columns: new[] { "EmployeeId", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProjectPerformances_ProjectId",
                table: "EmployeeProjectPerformances",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamCollaborations_Employee1Id_Employee2Id_ProjectId",
                table: "TeamCollaborations",
                columns: new[] { "Employee1Id", "Employee2Id", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamCollaborations_ProjectId",
                table: "TeamCollaborations",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeProjectPerformances");

            migrationBuilder.DropTable(
                name: "TeamCollaborations");
        }
    }
}
