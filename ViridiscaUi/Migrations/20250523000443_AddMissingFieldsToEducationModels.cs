using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViridiscaUi.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFieldsToEducationModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_groups_teachers_curator_uid",
                table: "groups");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "subjects",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "subjects",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "subjects",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "subjects",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "students",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "enrollment_date",
                table: "students",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_DATE",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "groups",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "groups",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "groups",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_hire_date",
                table: "teachers",
                column: "hire_date");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_status",
                table: "teachers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_code",
                table: "subjects",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subjects_is_active",
                table: "subjects",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_type",
                table: "subjects",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_students_enrollment_date",
                table: "students",
                column: "enrollment_date");

            migrationBuilder.CreateIndex(
                name: "ix_students_status",
                table: "students",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_groups_code",
                table: "groups",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_groups_start_date",
                table: "groups",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "ix_groups_status",
                table: "groups",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_groups_year",
                table: "groups",
                column: "year");

            migrationBuilder.AddForeignKey(
                name: "fk_groups_teachers_curator_uid",
                table: "groups",
                column: "curator_uid",
                principalTable: "teachers",
                principalColumn: "uid",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_groups_teachers_curator_uid",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "ix_teachers_hire_date",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "ix_teachers_status",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "ix_subjects_code",
                table: "subjects");

            migrationBuilder.DropIndex(
                name: "ix_subjects_is_active",
                table: "subjects");

            migrationBuilder.DropIndex(
                name: "ix_subjects_type",
                table: "subjects");

            migrationBuilder.DropIndex(
                name: "ix_students_enrollment_date",
                table: "students");

            migrationBuilder.DropIndex(
                name: "ix_students_status",
                table: "students");

            migrationBuilder.DropIndex(
                name: "ix_groups_code",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "ix_groups_start_date",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "ix_groups_status",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "ix_groups_year",
                table: "groups");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "subjects",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "subjects",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "subjects",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "subjects",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "students",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "enrollment_date",
                table: "students",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_DATE");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "groups",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "groups",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "groups",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddForeignKey(
                name: "fk_groups_teachers_curator_uid",
                table: "groups",
                column: "curator_uid",
                principalTable: "teachers",
                principalColumn: "uid");
        }
    }
}
