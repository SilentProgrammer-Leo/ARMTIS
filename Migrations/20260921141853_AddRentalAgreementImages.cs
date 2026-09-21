using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMTIS_Capstone_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalAgreementImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RentalAgreementImagePath",
                table: "Tenants");

            migrationBuilder.CreateTable(
                name: "RentalAgreementImages",
                columns: table => new
                {
                    RentalAgreementImageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantID = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalAgreementImages", x => x.RentalAgreementImageID);
                    table.ForeignKey(
                        name: "FK_RentalAgreementImages_Tenants_TenantID",
                        column: x => x.TenantID,
                        principalTable: "Tenants",
                        principalColumn: "TenantID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalAgreementImages_TenantID",
                table: "RentalAgreementImages",
                column: "TenantID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalAgreementImages");

            migrationBuilder.AddColumn<string>(
                name: "RentalAgreementImagePath",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
