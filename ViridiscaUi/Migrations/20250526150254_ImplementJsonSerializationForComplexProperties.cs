using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViridiscaUi.Migrations
{
    /// <inheritdoc />
    public partial class ImplementJsonSerializationForComplexProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_grades_lessons_lesson_uid1",
                table: "grades");

            migrationBuilder.DropForeignKey(
                name: "fk_lessons_groups_group_uid1",
                table: "lessons");

            migrationBuilder.DropForeignKey(
                name: "fk_lessons_modules_module_uid",
                table: "lessons");

            migrationBuilder.DropForeignKey(
                name: "fk_lessons_subjects_subject_uid1",
                table: "lessons");

            migrationBuilder.DropForeignKey(
                name: "fk_lessons_teachers_teacher_uid1",
                table: "lessons");

            migrationBuilder.DropIndex(
                name: "ix_lessons_group_uid1",
                table: "lessons");

            migrationBuilder.DropIndex(
                name: "ix_lessons_subject_uid1",
                table: "lessons");

            migrationBuilder.DropIndex(
                name: "ix_lessons_teacher_uid1",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "end_time",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "group_uid1",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "is_cancelled",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "start_time",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "subject_uid1",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "teacher_uid1",
                table: "lessons");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "modules",
                newName: "order_index");

            migrationBuilder.RenameColumn(
                name: "is_completed",
                table: "lessons",
                newName: "is_published");

            migrationBuilder.RenameColumn(
                name: "cancellation_reason",
                table: "lessons",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "lesson_uid1",
                table: "grades",
                newName: "assignment_uid");

            migrationBuilder.RenameIndex(
                name: "ix_grades_lesson_uid1",
                table: "grades",
                newName: "ix_grades_assignment_uid");

            migrationBuilder.AddColumn<Guid>(
                name: "assignment_id",
                table: "submissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "file_path",
                table: "submissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "student_id",
                table: "submissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "students",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "user_uid1",
                table: "students",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "action_url",
                table: "notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "expires_at",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_important",
                table: "notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "metadata",
                table: "notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "metadata_json",
                table: "notifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "repeat_interval",
                table: "notifications",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "scheduled_for",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_published",
                table: "modules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "teacher_uid",
                table: "lessons",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "subject_uid",
                table: "lessons",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "module_uid",
                table: "lessons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "group_uid",
                table: "lessons",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "content",
                table: "lessons",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "duration",
                table: "lessons",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "order_index",
                table: "lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_activity_date",
                table: "groups",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "courses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "course_id",
                table: "assignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "difficulty",
                table: "assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "instructions",
                table: "assignments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "module_id",
                table: "assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "module_uid",
                table: "assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "file_records",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    stored_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entity_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_records", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "lesson_progress",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    time_spent = table.Column<TimeSpan>(type: "interval", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_progress", x => x.uid);
                    table.ForeignKey(
                        name: "fk_lesson_progress_lessons_lesson_uid",
                        column: x => x.lesson_uid,
                        principalTable: "lessons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_progress_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_settings",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    email_notifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    push_notifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sms_notifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    quiet_hours_start = table.Column<TimeSpan>(type: "interval", nullable: false),
                    quiet_hours_end = table.Column<TimeSpan>(type: "interval", nullable: false),
                    weekend_notifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    minimum_priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    type_settings = table.Column<string>(type: "text", nullable: false),
                    category_settings = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_settings", x => x.uid);
                    table.ForeignKey(
                        name: "fk_notification_settings_users_user_uid",
                        column: x => x.user_uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_templates",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    title_template = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message_template = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    parameters = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_templates", x => x.uid);
                });

            migrationBuilder.CreateIndex(
                name: "ix_students_user_uid1",
                table: "students",
                column: "user_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_file_records_content_type",
                table: "file_records",
                column: "content_type");

            migrationBuilder.CreateIndex(
                name: "ix_file_records_created_at",
                table: "file_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_file_records_entity_type_entity_uid",
                table: "file_records",
                columns: new[] { "entity_type", "entity_uid" });

            migrationBuilder.CreateIndex(
                name: "ix_file_records_is_public",
                table: "file_records",
                column: "is_public");

            migrationBuilder.CreateIndex(
                name: "ix_file_records_uploaded_by_uid",
                table: "file_records",
                column: "uploaded_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progress_completed_at",
                table: "lesson_progress",
                column: "completed_at");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progress_is_completed",
                table: "lesson_progress",
                column: "is_completed");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progress_lesson_uid",
                table: "lesson_progress",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progress_student_uid_lesson_uid",
                table: "lesson_progress",
                columns: new[] { "student_uid", "lesson_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notification_settings_user_uid",
                table: "notification_settings",
                column: "user_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notification_templates_is_active",
                table: "notification_templates",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_notification_templates_name",
                table: "notification_templates",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_notification_templates_type",
                table: "notification_templates",
                column: "type");

            migrationBuilder.AddForeignKey(
                name: "fk_grades_assignments_assignment_uid",
                table: "grades",
                column: "assignment_uid",
                principalTable: "assignments",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_lessons_modules_module_uid",
                table: "lessons",
                column: "module_uid",
                principalTable: "modules",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_students_users_user_uid1",
                table: "students",
                column: "user_uid1",
                principalTable: "users",
                principalColumn: "uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_grades_assignments_assignment_uid",
                table: "grades");

            migrationBuilder.DropForeignKey(
                name: "fk_lessons_modules_module_uid",
                table: "lessons");

            migrationBuilder.DropForeignKey(
                name: "fk_students_users_user_uid1",
                table: "students");

            migrationBuilder.DropTable(
                name: "file_records");

            migrationBuilder.DropTable(
                name: "lesson_progress");

            migrationBuilder.DropTable(
                name: "notification_settings");

            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropIndex(
                name: "ix_students_user_uid1",
                table: "students");

            migrationBuilder.DropColumn(
                name: "assignment_id",
                table: "submissions");

            migrationBuilder.DropColumn(
                name: "file_path",
                table: "submissions");

            migrationBuilder.DropColumn(
                name: "student_id",
                table: "submissions");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "students");

            migrationBuilder.DropColumn(
                name: "user_uid1",
                table: "students");

            migrationBuilder.DropColumn(
                name: "action_url",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "category",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "expires_at",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "is_important",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "metadata",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "metadata_json",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "repeat_interval",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "scheduled_for",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "is_published",
                table: "modules");

            migrationBuilder.DropColumn(
                name: "content",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "duration",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "order_index",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "type",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "last_activity_date",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "title",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "course_id",
                table: "assignments");

            migrationBuilder.DropColumn(
                name: "difficulty",
                table: "assignments");

            migrationBuilder.DropColumn(
                name: "instructions",
                table: "assignments");

            migrationBuilder.DropColumn(
                name: "module_id",
                table: "assignments");

            migrationBuilder.DropColumn(
                name: "module_uid",
                table: "assignments");

            migrationBuilder.DropColumn(
                name: "status",
                table: "assignments");

            migrationBuilder.RenameColumn(
                name: "order_index",
                table: "modules",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "lessons",
                newName: "cancellation_reason");

            migrationBuilder.RenameColumn(
                name: "is_published",
                table: "lessons",
                newName: "is_completed");

            migrationBuilder.RenameColumn(
                name: "assignment_uid",
                table: "grades",
                newName: "lesson_uid1");

            migrationBuilder.RenameIndex(
                name: "ix_grades_assignment_uid",
                table: "grades",
                newName: "ix_grades_lesson_uid1");

            migrationBuilder.AlterColumn<Guid>(
                name: "teacher_uid",
                table: "lessons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "subject_uid",
                table: "lessons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "module_uid",
                table: "lessons",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "group_uid",
                table: "lessons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "end_time",
                table: "lessons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "group_uid1",
                table: "lessons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_cancelled",
                table: "lessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_time",
                table: "lessons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "subject_uid1",
                table: "lessons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "teacher_uid1",
                table: "lessons",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_lessons_group_uid1",
                table: "lessons",
                column: "group_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_subject_uid1",
                table: "lessons",
                column: "subject_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_teacher_uid1",
                table: "lessons",
                column: "teacher_uid1");

            migrationBuilder.AddForeignKey(
                name: "fk_grades_lessons_lesson_uid1",
                table: "grades",
                column: "lesson_uid1",
                principalTable: "lessons",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_lessons_groups_group_uid1",
                table: "lessons",
                column: "group_uid1",
                principalTable: "groups",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_lessons_modules_module_uid",
                table: "lessons",
                column: "module_uid",
                principalTable: "modules",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_lessons_subjects_subject_uid1",
                table: "lessons",
                column: "subject_uid1",
                principalTable: "subjects",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_lessons_teachers_teacher_uid1",
                table: "lessons",
                column: "teacher_uid1",
                principalTable: "teachers",
                principalColumn: "uid");
        }
    }
}
