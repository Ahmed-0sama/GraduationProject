using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Migrations
{
    /// <inheritdoc />
    public partial class Pricehistoryupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "BestPriceProducts",
                newName: "CuurentPrice");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "BestPriceProducts",
                newName: "CurrentDate");

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "ProductPriceHistories",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateRecorded",
                table: "ProductPriceHistories",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CuurentPrice",
                table: "BestPriceProducts",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "CurrentDate",
                table: "BestPriceProducts",
                newName: "Date");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "ProductPriceHistories",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateRecorded",
                table: "ProductPriceHistories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }
    }
}
