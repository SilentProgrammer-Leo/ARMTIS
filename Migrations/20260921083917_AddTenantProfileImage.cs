using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMTIS_Capstone_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantProfileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePath",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImagePath",
                table: "Tenants");
        }
    }
}