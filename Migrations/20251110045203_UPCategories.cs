using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be.Migrations
{
    /// <inheritdoc />
    public partial class UPCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_EducatorUserId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_EducatorUserId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Educator",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "EducatorUserId",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Educator",
                table: "Categories",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EducatorUserId",
                table: "Categories",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_EducatorUserId",
                table: "Categories",
                column: "EducatorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_EducatorUserId",
                table: "Categories",
                column: "EducatorUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
