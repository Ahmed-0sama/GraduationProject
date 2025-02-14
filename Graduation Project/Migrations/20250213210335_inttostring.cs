using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Graduation_Project.Migrations
{
    /// <inheritdoc />
    public partial class inttostring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToBuyLists_AspNetUsers_UserId1",
                table: "ToBuyLists");

            migrationBuilder.DropIndex(
                name: "IX_ToBuyLists_UserId1",
                table: "ToBuyLists");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "ToBuyLists");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ToBuyLists",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToBuyLists_UserId",
                table: "ToBuyLists",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToBuyLists_AspNetUsers_UserId",
                table: "ToBuyLists",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToBuyLists_AspNetUsers_UserId",
                table: "ToBuyLists");

            migrationBuilder.DropIndex(
                name: "IX_ToBuyLists_UserId",
                table: "ToBuyLists");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ToBuyLists",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "ToBuyLists",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToBuyLists_UserId1",
                table: "ToBuyLists",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ToBuyLists_AspNetUsers_UserId1",
                table: "ToBuyLists",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
