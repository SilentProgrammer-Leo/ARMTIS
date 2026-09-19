using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMTIS_Capstone_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitToBillingStatement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingStatements_Tenants_TenantID",
                table: "BillingStatements");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Units_UnitID",
                table: "Tenants");

            migrationBuilder.AddColumn<int>(
                name: "UnitID",
                table: "BillingStatements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BillingStatements_UnitID",
                table: "BillingStatements",
                column: "UnitID");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingStatements_Tenants_TenantID",
                table: "BillingStatements",
                column: "TenantID",
                principalTable: "Tenants",
                principalColumn: "TenantID");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingStatements_Units_UnitID",
                table: "BillingStatements",
                column: "UnitID",
                principalTable: "Units",
                principalColumn: "UnitID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Units_UnitID",
                table: "Tenants",
                column: "UnitID",
                principalTable: "Units",
                principalColumn: "UnitID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingStatements_Tenants_TenantID",
                table: "BillingStatements");

            migrationBuilder.DropForeignKey(
                name: "FK_BillingStatements_Units_UnitID",
                table: "BillingStatements");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Units_UnitID",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_BillingStatements_UnitID",
                table: "BillingStatements");

            migrationBuilder.DropColumn(
                name: "UnitID",
                table: "BillingStatements");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingStatements_Tenants_TenantID",
                table: "BillingStatements",
                column: "TenantID",
                principalTable: "Tenants",
                principalColumn: "TenantID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Units_UnitID",
                table: "Tenants",
                column: "UnitID",
                principalTable: "Units",
                principalColumn: "UnitID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
