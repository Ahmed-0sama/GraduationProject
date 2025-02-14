using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Migrations
{
    /// <inheritdoc />
    public partial class hiii : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPriceHistories_BestPriceProducts_ProductItemId",
                table: "ProductPriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_ProductPriceHistories_ProductItemId",
                table: "ProductPriceHistories");

            migrationBuilder.DropColumn(
                name: "ProductItemId",
                table: "ProductPriceHistories");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceHistories_ItemId",
                table: "ProductPriceHistories",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPriceHistories_BestPriceProducts_ItemId",
                table: "ProductPriceHistories",
                column: "ItemId",
                principalTable: "BestPriceProducts",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPriceHistories_BestPriceProducts_ItemId",
                table: "ProductPriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_ProductPriceHistories_ItemId",
                table: "ProductPriceHistories");

            migrationBuilder.AddColumn<int>(
                name: "ProductItemId",
                table: "ProductPriceHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceHistories_ProductItemId",
                table: "ProductPriceHistories",
                column: "ProductItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPriceHistories_BestPriceProducts_ProductItemId",
                table: "ProductPriceHistories",
                column: "ProductItemId",
                principalTable: "BestPriceProducts",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
