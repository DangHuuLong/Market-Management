using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3_HK4.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBillStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confirm",
                table: "Bills");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Bills",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Bills");

            migrationBuilder.AddColumn<bool>(
                name: "Confirm",
                table: "Bills",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
