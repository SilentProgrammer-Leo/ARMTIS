using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMTIS_Capstone_Project.Migrations
{
    /// <inheritdoc />
    public partial class AutomateNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Tenants_TenantID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TenantID",
                table: "Notifications");

            migrationBuilder.AlterColumn<int>(
                name: "TenantID",
                table: "Notifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "RecipientUserID",
                table: "Notifications",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserID",
                table: "Notifications",
                column: "RecipientUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TenantID",
                table: "Notifications",
                column: "TenantID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_RecipientUserID",
                table: "Notifications",
                column: "RecipientUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Tenants_TenantID",
                table: "Notifications",
                column: "TenantID",
                principalTable: "Tenants",
                principalColumn: "TenantID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_RecipientUserID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Tenants_TenantID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RecipientUserID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TenantID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RecipientUserID",
                table: "Notifications");

            migrationBuilder.AlterColumn<int>(
                name: "TenantID",
                table: "Notifications",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TenantID",
                table: "Notifications",
                column: "TenantID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Tenants_TenantID",
                table: "Notifications",
                column: "TenantID",
                principalTable: "Tenants",
                principalColumn: "TenantID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
