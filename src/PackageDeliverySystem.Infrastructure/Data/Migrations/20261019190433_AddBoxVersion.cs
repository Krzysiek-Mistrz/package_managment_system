using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageDeliverySystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBoxVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Boxes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Boxes");
        }
    }
}
