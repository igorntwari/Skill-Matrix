using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatchPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectRequirements_ProjectId",
                table: "ProjectRequirements");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequirement_Unique_Skill_Proficiency",
                table: "ProjectRequirements",
                columns: new[] { "ProjectId", "SkillId", "MinimumProficiency" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectRequirement_Unique_Skill_Proficiency",
                table: "ProjectRequirements");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRequirements_ProjectId",
                table: "ProjectRequirements",
                column: "ProjectId");
        }
    }
}
