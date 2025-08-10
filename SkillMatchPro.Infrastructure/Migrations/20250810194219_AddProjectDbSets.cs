using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatchPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectDbSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectAssignment_Employees_EmployeeId",
                table: "ProjectAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectAssignment_Project_ProjectId",
                table: "ProjectAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequirement_Project_ProjectId",
                table: "ProjectRequirement");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequirement_Skills_SkillId",
                table: "ProjectRequirement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectRequirement",
                table: "ProjectRequirement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectAssignment",
                table: "ProjectAssignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Project",
                table: "Project");

            migrationBuilder.RenameTable(
                name: "ProjectRequirement",
                newName: "ProjectRequirements");

            migrationBuilder.RenameTable(
                name: "ProjectAssignment",
                newName: "ProjectAssignments");

            migrationBuilder.RenameTable(
                name: "Project",
                newName: "Projects");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectRequirement_SkillId",
                table: "ProjectRequirements",
                newName: "IX_ProjectRequirements_SkillId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectRequirement_ProjectId",
                table: "ProjectRequirements",
                newName: "IX_ProjectRequirements_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectAssignment_ProjectId",
                table: "ProjectAssignments",
                newName: "IX_ProjectAssignments_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectAssignment_EmployeeId_ProjectId_IsActive",
                table: "ProjectAssignments",
                newName: "IX_ProjectAssignments_EmployeeId_ProjectId_IsActive");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Projects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectRequirements",
                table: "ProjectRequirements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectAssignments",
                table: "ProjectAssignments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectAssignments_Employees_EmployeeId",
                table: "ProjectAssignments",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectAssignments_Projects_ProjectId",
                table: "ProjectAssignments",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequirements_Projects_ProjectId",
                table: "ProjectRequirements",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequirements_Skills_SkillId",
                table: "ProjectRequirements",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectAssignments_Employees_EmployeeId",
                table: "ProjectAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectAssignments_Projects_ProjectId",
                table: "ProjectAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequirements_Projects_ProjectId",
                table: "ProjectRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectRequirements_Skills_SkillId",
                table: "ProjectRequirements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectRequirements",
                table: "ProjectRequirements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectAssignments",
                table: "ProjectAssignments");

            migrationBuilder.RenameTable(
                name: "Projects",
                newName: "Project");

            migrationBuilder.RenameTable(
                name: "ProjectRequirements",
                newName: "ProjectRequirement");

            migrationBuilder.RenameTable(
                name: "ProjectAssignments",
                newName: "ProjectAssignment");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectRequirements_SkillId",
                table: "ProjectRequirement",
                newName: "IX_ProjectRequirement_SkillId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectRequirements_ProjectId",
                table: "ProjectRequirement",
                newName: "IX_ProjectRequirement_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectAssignments_ProjectId",
                table: "ProjectAssignment",
                newName: "IX_ProjectAssignment_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectAssignments_EmployeeId_ProjectId_IsActive",
                table: "ProjectAssignment",
                newName: "IX_ProjectAssignment_EmployeeId_ProjectId_IsActive");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Project",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project",
                table: "Project",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectRequirement",
                table: "ProjectRequirement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectAssignment",
                table: "ProjectAssignment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectAssignment_Employees_EmployeeId",
                table: "ProjectAssignment",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectAssignment_Project_ProjectId",
                table: "ProjectAssignment",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequirement_Project_ProjectId",
                table: "ProjectRequirement",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectRequirement_Skills_SkillId",
                table: "ProjectRequirement",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
