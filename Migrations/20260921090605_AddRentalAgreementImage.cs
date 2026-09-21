using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMTIS_Capstone_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalAgreementImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RentalAgreementImagePath",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RentalAgreementImagePath",
                table: "Tenants");
        }
    }
}
