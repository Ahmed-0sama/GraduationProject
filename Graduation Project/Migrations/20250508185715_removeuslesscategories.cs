using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Migrations
{
    /// <inheritdoc />
    public partial class removeuslesscategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "BestPriceProducts");

            migrationBuilder.DropColumn(
                name: "IsBought",
                table: "BestPriceProducts");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "BestPriceProducts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "BestPriceProducts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBought",
                table: "BestPriceProducts",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "BestPriceProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
