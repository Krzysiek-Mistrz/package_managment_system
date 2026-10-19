using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PackageDeliverySystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageDestinationBox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DestinationBoxId",
                table: "Packages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Packages_DestinationBoxId",
                table: "Packages",
                column: "DestinationBoxId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Boxes_DestinationBoxId",
                table: "Packages",
                column: "DestinationBoxId",
                principalTable: "Boxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Boxes_DestinationBoxId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_DestinationBoxId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DestinationBoxId",
                table: "Packages");
        }
    }
}
