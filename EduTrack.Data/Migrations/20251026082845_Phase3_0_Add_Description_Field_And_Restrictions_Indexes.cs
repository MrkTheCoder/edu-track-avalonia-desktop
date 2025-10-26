using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_0_Add_Description_Field_And_Restrictions_Indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Courses",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_LastName",
                table: "Students",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Name",
                table: "Courses",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_Email",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_LastName",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Name",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Courses");
        }
    }
}
