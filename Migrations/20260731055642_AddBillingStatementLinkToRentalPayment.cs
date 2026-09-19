using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMTIS_Capstone_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingStatementLinkToRentalPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
             name: "BillingStatementID",
             table: "RentalPayments",
             type: "int",
             nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RentalPayments_BillingStatementID",
                table: "RentalPayments",
                column: "BillingStatementID");

            migrationBuilder.AddForeignKey(
                name: "FK_RentalPayments_BillingStatements_BillingStatementID",
                table: "RentalPayments",
                column: "BillingStatementID",
                principalTable: "BillingStatements",
                principalColumn: "BillingStatementID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
               name: "FK_RentalPayments_BillingStatements_BillingStatementID",
               table: "RentalPayments");

            migrationBuilder.DropIndex(
                name: "IX_RentalPayments_BillingStatementID",
                table: "RentalPayments");

            migrationBuilder.DropColumn(
                name: "BillingStatementID",
                table: "RentalPayments");
        }
    }
}
