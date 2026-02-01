using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace be.Migrations
{
    /// <inheritdoc />
    public partial class Modify_CourseProgresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseProgresses_Courses_CourseId",
                table: "CourseProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseProgresses_Users_UserId",
                table: "CourseProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_CourseProgresses_UserId",
                table: "CourseProgresses");

            migrationBuilder.DropColumn(
                name: "Completed",
                table: "CourseProgresses");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CourseProgresses");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "CourseProgresses",
                newName: "LectureId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseProgresses_CourseId",
                table: "CourseProgresses",
                newName: "IX_CourseProgresses_LectureId");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "CourseProgresses",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EnrollmentId",
                table: "CourseProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CourseProgresses_ApplicationUserId",
                table: "CourseProgresses",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseProgresses_EnrollmentId_LectureId",
                table: "CourseProgresses",
                columns: new[] { "EnrollmentId", "LectureId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseProgresses_Enrollments_EnrollmentId",
                table: "CourseProgresses",
                column: "EnrollmentId",
                principalTable: "Enrollments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseProgresses_Lectures_LectureId",
                table: "CourseProgresses",
                column: "LectureId",
                principalTable: "Lectures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseProgresses_Users_ApplicationUserId",
                table: "CourseProgresses",
                column: "ApplicationUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseProgresses_Enrollments_EnrollmentId",
                table: "CourseProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseProgresses_Lectures_LectureId",
                table: "CourseProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseProgresses_Users_ApplicationUserId",
                table: "CourseProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_CourseProgresses_ApplicationUserId",
                table: "CourseProgresses");

            migrationBuilder.DropIndex(
                name: "IX_CourseProgresses_EnrollmentId_LectureId",
                table: "CourseProgresses");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "CourseProgresses");

            migrationBuilder.DropColumn(
                name: "EnrollmentId",
                table: "CourseProgresses");

            migrationBuilder.RenameColumn(
                name: "LectureId",
                table: "CourseProgresses",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseProgresses_LectureId",
                table: "CourseProgresses",
                newName: "IX_CourseProgresses_CourseId");

            migrationBuilder.AddColumn<bool>(
                name: "Completed",
                table: "CourseProgresses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "CourseProgresses",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CourseProgresses_UserId",
                table: "CourseProgresses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseProgresses_Courses_CourseId",
                table: "CourseProgresses",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseProgresses_Users_UserId",
                table: "CourseProgresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Courses_CourseId",
                table: "Enrollments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
