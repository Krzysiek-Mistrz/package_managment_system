using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageDeliverySystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPickupCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Rank",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupCode",
                table: "Packages",
                type: "varchar(6)",
                maxLength: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PickupCode",
                table: "Packages");

            migrationBuilder.AlterColumn<double>(
                name: "Rank",
                table: "Users",
                type: "double",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
