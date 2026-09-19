using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMTIS_Capstone_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilityReadingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UtilityReadings",
                columns: table => new
                {
                    UtilityReadingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantID = table.Column<int>(type: "int", nullable: false),
                    UnitID = table.Column<int>(type: "int", nullable: false),
                    BillingMonth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreviousElectricReading = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentElectricReading = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ElectricConsumption = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ElectricRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ElectricityBill = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousWaterReading = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentWaterReading = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WaterConsumption = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WaterMinimumCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WaterBill = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InternetBill = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalUtilityCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateRecorded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilityReadings", x => x.UtilityReadingID);
                    table.ForeignKey(
                        name: "FK_UtilityReadings_Tenants_TenantID",
                        column: x => x.TenantID,
                        principalTable: "Tenants",
                        principalColumn: "TenantID");
                    table.ForeignKey(
                        name: "FK_UtilityReadings_Units_UnitID",
                        column: x => x.UnitID,
                        principalTable: "Units",
                        principalColumn: "UnitID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UtilityReadings_TenantID",
                table: "UtilityReadings",
                column: "TenantID");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityReadings_UnitID",
                table: "UtilityReadings",
                column: "UnitID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UtilityReadings");
        }
    }
}
