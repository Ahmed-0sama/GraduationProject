using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Migrations
{
    /// <inheritdoc />
    public partial class PriceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BestPriceProducts_ToBuyLists_ListId",
                table: "BestPriceProducts");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "BestPriceProducts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "BestPriceProducts",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ListId",
                table: "BestPriceProducts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ProductPriceHistories",
                columns: table => new
                {
                    PriceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateRecorded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPriceHistories", x => x.PriceID);
                    table.ForeignKey(
                        name: "FK_ProductPriceHistories_BestPriceProducts_ProductItemId",
                        column: x => x.ProductItemId,
                        principalTable: "BestPriceProducts",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceHistories_ProductItemId",
                table: "ProductPriceHistories",
                column: "ProductItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_BestPriceProducts_ToBuyLists_ListId",
                table: "BestPriceProducts",
                column: "ListId",
                principalTable: "ToBuyLists",
                principalColumn: "ListId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BestPriceProducts_ToBuyLists_ListId",
                table: "BestPriceProducts");

            migrationBuilder.DropTable(
                name: "ProductPriceHistories");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "BestPriceProducts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "BestPriceProducts",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "ListId",
                table: "BestPriceProducts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_BestPriceProducts_ToBuyLists_ListId",
                table: "BestPriceProducts",
                column: "ListId",
                principalTable: "ToBuyLists",
                principalColumn: "ListId");
        }
    }
}
