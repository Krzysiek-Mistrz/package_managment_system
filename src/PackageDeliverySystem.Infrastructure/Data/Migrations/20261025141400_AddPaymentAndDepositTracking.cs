using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace PackageDeliverySystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAndDepositTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DepositConfirmedAt",
                table: "Packages",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepositDeadline",
                table: "Packages",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    PayerEmail = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    ExternalReference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PackageId",
                table: "Payments",
                column: "PackageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropColumn(
                name: "DepositConfirmedAt",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DepositDeadline",
                table: "Packages");
        }
    }
}
