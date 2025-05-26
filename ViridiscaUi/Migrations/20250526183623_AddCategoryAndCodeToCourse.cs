using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViridiscaUi.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndCodeToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "teachers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "parent_uid",
                table: "student_parents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "teacher_uid",
                table: "groups",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "comment",
                table: "grades",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "graded_at",
                table: "grades",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "graded_by_uid",
                table: "grades",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "max_grade",
                table: "assignments",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "ix_groups_teacher_uid",
                table: "groups",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_graded_by_uid",
                table: "grades",
                column: "graded_by_uid");

            migrationBuilder.AddForeignKey(
                name: "fk_grades_teachers_graded_by_uid",
                table: "grades",
                column: "graded_by_uid",
                principalTable: "teachers",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_groups_teachers_teacher_uid",
                table: "groups",
                column: "teacher_uid",
                principalTable: "teachers",
                principalColumn: "uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_grades_teachers_graded_by_uid",
                table: "grades");

            migrationBuilder.DropForeignKey(
                name: "fk_groups_teachers_teacher_uid",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "ix_groups_teacher_uid",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "ix_grades_graded_by_uid",
                table: "grades");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "parent_uid",
                table: "student_parents");

            migrationBuilder.DropColumn(
                name: "teacher_uid",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "comment",
                table: "grades");

            migrationBuilder.DropColumn(
                name: "graded_at",
                table: "grades");

            migrationBuilder.DropColumn(
                name: "graded_by_uid",
                table: "grades");

            migrationBuilder.DropColumn(
                name: "category",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "code",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "max_grade",
                table: "assignments");
        }
    }
}
