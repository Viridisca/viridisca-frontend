using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ViridiscaUi.Migrations
{
    /// <inheritdoc />
    public partial class CompleteViridiscaLmsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "profile_image_url",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "middle_name",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "last_name",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "first_name",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "role_id",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "department_uid",
                table: "teachers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "user_uid1",
                table: "teachers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "attendances",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    checked_by_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendances", x => x.uid);
                    table.ForeignKey(
                        name: "fk_attendances_lessons_lesson_uid",
                        column: x => x.lesson_uid,
                        principalTable: "lessons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_attendances_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_attendances_teachers_checked_by_uid",
                        column: x => x.checked_by_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    head_of_department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_departments", x => x.uid);
                    table.ForeignKey(
                        name: "fk_departments_teachers_head_of_department_uid",
                        column: x => x.head_of_department_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "lesson_details",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    teacher_first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    teacher_last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    teacher_middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    subject_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    group_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_cancelled = table.Column<bool>(type: "boolean", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_details", x => x.uid);
                    table.ForeignKey(
                        name: "fk_lesson_details_lessons_lesson_uid",
                        column: x => x.lesson_uid,
                        principalTable: "lessons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.uid);
                    table.ForeignKey(
                        name: "fk_notifications_users_recipient_uid",
                        column: x => x.recipient_uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schedules",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    classroom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedules", x => x.uid);
                    table.ForeignKey(
                        name: "fk_schedules_groups_group_uid",
                        column: x => x.group_uid,
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_schedules_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_schedules_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "uid", "code", "created_at", "description", "head_of_department_uid", "is_active", "last_modified_at", "name" },
                values: new object[,]
                {
                    { new Guid("66666666-6666-6666-6666-666666666666"), "IT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кафедра информационных технологий и программирования", null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Информационные технологии" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "MATH", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кафедра высшей математики и математического анализа", null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Математический анализ" }
                });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "uid",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "date_of_birth", "is_email_confirmed", "role_id" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.CreateIndex(
                name: "ix_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_department_uid",
                table: "teachers",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_user_uid1",
                table: "teachers",
                column: "user_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_department_uid",
                table: "subjects",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_groups_department_uid",
                table: "groups",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_attendances_checked_by_uid",
                table: "attendances",
                column: "checked_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_attendances_lesson_uid",
                table: "attendances",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_attendances_student_uid_lesson_uid",
                table: "attendances",
                columns: new[] { "student_uid", "lesson_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_code",
                table: "departments",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_head_of_department_uid",
                table: "departments",
                column: "head_of_department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_details_lesson_uid",
                table: "lesson_details",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_details_start_time",
                table: "lesson_details",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_details_teacher_last_name_teacher_first_name",
                table: "lesson_details",
                columns: new[] { "teacher_last_name", "teacher_first_name" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_recipient_uid_is_read",
                table: "notifications",
                columns: new[] { "recipient_uid", "is_read" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_sent_at",
                table: "notifications",
                column: "sent_at");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_type",
                table: "notifications",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_schedules_group_uid_day_of_week_start_time",
                table: "schedules",
                columns: new[] { "group_uid", "day_of_week", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_schedules_subject_uid",
                table: "schedules",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_schedules_teacher_uid_day_of_week_start_time",
                table: "schedules",
                columns: new[] { "teacher_uid", "day_of_week", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_schedules_valid_from",
                table: "schedules",
                column: "valid_from");

            migrationBuilder.AddForeignKey(
                name: "fk_groups_departments_department_uid",
                table: "groups",
                column: "department_uid",
                principalTable: "departments",
                principalColumn: "uid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_subjects_departments_department_uid",
                table: "subjects",
                column: "department_uid",
                principalTable: "departments",
                principalColumn: "uid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_teachers_departments_department_uid",
                table: "teachers",
                column: "department_uid",
                principalTable: "departments",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_teachers_users_user_uid1",
                table: "teachers",
                column: "user_uid1",
                principalTable: "users",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_users_roles_role_id",
                table: "users",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "uid",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_groups_departments_department_uid",
                table: "groups");

            migrationBuilder.DropForeignKey(
                name: "fk_subjects_departments_department_uid",
                table: "subjects");

            migrationBuilder.DropForeignKey(
                name: "fk_teachers_departments_department_uid",
                table: "teachers");

            migrationBuilder.DropForeignKey(
                name: "fk_teachers_users_user_uid1",
                table: "teachers");

            migrationBuilder.DropForeignKey(
                name: "fk_users_roles_role_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "attendances");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "lesson_details");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "schedules");

            migrationBuilder.DropIndex(
                name: "ix_users_role_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_teachers_department_uid",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "ix_teachers_user_uid1",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "ix_subjects_department_uid",
                table: "subjects");

            migrationBuilder.DropIndex(
                name: "ix_groups_department_uid",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "role_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "department_uid",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "user_uid1",
                table: "teachers");

            migrationBuilder.AlterColumn<string>(
                name: "profile_image_url",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "middle_name",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "last_name",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "first_name",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "uid",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "date_of_birth", "is_email_confirmed" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false });
        }
    }
}
