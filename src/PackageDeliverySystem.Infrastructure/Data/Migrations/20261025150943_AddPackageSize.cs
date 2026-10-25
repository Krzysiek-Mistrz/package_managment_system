using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageDeliverySystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Packages",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "Packages");
        }
    }
}
