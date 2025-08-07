using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillMatchPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserRoleToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert string roles to integer enum values
            migrationBuilder.Sql(@"
                ALTER TABLE ""Users"" 
                ALTER COLUMN ""Role"" TYPE integer 
                USING CASE 
                    WHEN ""Role"" = 'Employee' THEN 1
                    WHEN ""Role"" = 'Manager' THEN 2
                    WHEN ""Role"" = 'Admin' THEN 3
                    ELSE 1
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert back to string for rollback
            migrationBuilder.Sql(@"
                ALTER TABLE ""Users"" 
                ALTER COLUMN ""Role"" TYPE text 
                USING CASE 
                    WHEN ""Role"" = 1 THEN 'Employee'
                    WHEN ""Role"" = 2 THEN 'Manager'
                    WHEN ""Role"" = 3 THEN 'Admin'
                    ELSE 'Employee'
                END;
            ");
        }
    }
}