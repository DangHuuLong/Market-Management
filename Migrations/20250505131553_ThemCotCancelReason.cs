using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3_HK4.Migrations
{
    /// <inheritdoc />
    public partial class ThemCotCancelReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellingReason",
                table: "Bills",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellingReason",
                table: "Bills");
        }
    }
}
