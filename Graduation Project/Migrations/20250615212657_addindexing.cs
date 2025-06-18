using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Migrations
{
    /// <inheritdoc />
    public partial class addindexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchasedProducts_UserId",
                table: "PurchasedProducts");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyBills_UserId",
                table: "MonthlyBills");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedProducts_UserId_Date",
                table: "PurchasedProducts",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyBills_UserId_EndDate",
                table: "MonthlyBills",
                columns: new[] { "UserId", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchasedProducts_UserId_Date",
                table: "PurchasedProducts");

            migrationBuilder.DropIndex(
                name: "IX_MonthlyBills_UserId_EndDate",
                table: "MonthlyBills");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedProducts_UserId",
                table: "PurchasedProducts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyBills_UserId",
                table: "MonthlyBills",
                column: "UserId");
        }
    }
}
