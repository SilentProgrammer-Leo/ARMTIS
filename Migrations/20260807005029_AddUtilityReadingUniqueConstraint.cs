using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMTIS_Capstone_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilityReadingUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UtilityReadings_TenantID",
                table: "UtilityReadings");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityReadings_TenantID_BillingMonth",
                table: "UtilityReadings",
                columns: new[] { "TenantID", "BillingMonth" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UtilityReadings_TenantID_BillingMonth",
                table: "UtilityReadings");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityReadings_TenantID",
                table: "UtilityReadings",
                column: "TenantID");
        }
    }
}
