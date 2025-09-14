using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBereavementAbsenceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing Bereavement (3) records to Other (4) before we change the enum values
            migrationBuilder.Sql("UPDATE AbsenceRequests SET Type = 4 WHERE Type = 3");
            
            // Update existing Other (4) records to the new Other value (3)
            migrationBuilder.Sql("UPDATE AbsenceRequests SET Type = 3 WHERE Type = 4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the migration: restore the original enum values
            // First, move current Other (3) back to Other (4)
            migrationBuilder.Sql("UPDATE AbsenceRequests SET Type = 4 WHERE Type = 3");
            
            // Note: We cannot restore the original Bereavement records since we don't know which
            // "Other" records were originally "Bereavement". This is a destructive migration.
            // In a production environment, you might want to add a backup column first.
        }
    }
}
