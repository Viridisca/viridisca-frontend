using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViridiscaUi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithoutSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auth_tokens",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    refresh_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auth_tokens", x => x.uid);
                });

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

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    role_type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    role_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    role_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    permission_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => x.uid);
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_uid",
                        column: x => x.permission_uid,
                        principalTable: "permissions",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_uid1",
                        column: x => x.permission_uid1,
                        principalTable: "permissions",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_uid",
                        column: x => x.role_uid,
                        principalTable: "roles",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_uid1",
                        column: x => x.role_uid1,
                        principalTable: "roles",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    profile_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.uid);
                    table.ForeignKey(
                        name: "fk_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
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
                    category = table.Column<string>(type: "text", nullable: true),
                    action_url = table.Column<string>(type: "text", nullable: true),
                    is_important = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    scheduled_for = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    repeat_interval = table.Column<TimeSpan>(type: "interval", nullable: true),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    metadata_json = table.Column<string>(type: "text", nullable: true),
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
                name: "user_roles",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    role_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.uid);
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_uid",
                        column: x => x.role_uid,
                        principalTable: "roles",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_uid",
                        column: x => x.user_uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_uid1",
                        column: x => x.user_uid1,
                        principalTable: "users",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "assignments",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_score = table.Column<double>(type: "double precision", nullable: false),
                    max_grade = table.Column<double>(type: "double precision", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    course_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    course_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    lesson_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    instructions = table.Column<string>(type: "text", nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    module_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignments", x => x.uid);
                });

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
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    teacher_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    prerequisites = table.Column<string>(type: "text", nullable: false),
                    learning_outcomes = table.Column<string>(type: "text", nullable: false),
                    max_enrollments = table.Column<int>(type: "integer", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_courses", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "modules",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    course_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_modules", x => x.uid);
                    table.ForeignKey(
                        name: "fk_modules_courses_course_uid",
                        column: x => x.course_uid,
                        principalTable: "courses",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_modules_courses_course_uid1",
                        column: x => x.course_uid1,
                        principalTable: "courses",
                        principalColumn: "uid");
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
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    lessons_per_week = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    department_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subjects", x => x.uid);
                    table.ForeignKey(
                        name: "fk_subjects_departments_department_uid",
                        column: x => x.department_uid,
                        principalTable: "departments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subjects_departments_department_uid1",
                        column: x => x.department_uid1,
                        principalTable: "departments",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "teachers",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hire_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    termination_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    academic_degree = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    academic_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    specialization = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    hourly_rate = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    phone = table.Column<string>(type: "text", nullable: false),
                    user_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teachers", x => x.uid);
                    table.ForeignKey(
                        name: "fk_teachers_departments_department_uid",
                        column: x => x.department_uid,
                        principalTable: "departments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_teachers_users_user_uid",
                        column: x => x.user_uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_teachers_users_user_uid1",
                        column: x => x.user_uid1,
                        principalTable: "users",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_students = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    curator_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    last_activity_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.uid);
                    table.ForeignKey(
                        name: "fk_groups_departments_department_uid",
                        column: x => x.department_uid,
                        principalTable: "departments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_groups_teachers_curator_uid",
                        column: x => x.curator_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_groups_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "teacher_subjects",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    is_main_teacher = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deactivated_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher_subjects", x => x.uid);
                    table.ForeignKey(
                        name: "fk_teacher_subjects_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_teacher_subjects_subjects_subject_uid1",
                        column: x => x.subject_uid1,
                        principalTable: "subjects",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_teacher_subjects_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_teacher_subjects_teachers_teacher_uid1",
                        column: x => x.teacher_uid1,
                        principalTable: "teachers",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    module_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lessons", x => x.uid);
                    table.ForeignKey(
                        name: "fk_lessons_groups_group_uid",
                        column: x => x.group_uid,
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_lessons_modules_module_uid",
                        column: x => x.module_uid,
                        principalTable: "modules",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lessons_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lessons_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
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

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    student_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    enrollment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    status = table.Column<int>(type: "integer", nullable: false),
                    emergency_contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    emergency_contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    medical_information = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    graduation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_students", x => x.uid);
                    table.ForeignKey(
                        name: "fk_students_groups_group_uid",
                        column: x => x.group_uid,
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_students_users_user_uid",
                        column: x => x.user_uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_students_users_user_uid1",
                        column: x => x.user_uid1,
                        principalTable: "users",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "teacher_groups",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    is_curator = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    teacher_uid1 = table.Column<Guid>(type: "uuid", nullable: false),
                    group_uid1 = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid1 = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher_groups", x => x.uid);
                    table.ForeignKey(
                        name: "fk_teacher_groups_groups_group_uid",
                        column: x => x.group_uid,
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_teacher_groups_groups_group_uid1",
                        column: x => x.group_uid1,
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_teacher_groups_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_teacher_groups_subjects_subject_uid1",
                        column: x => x.subject_uid1,
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_teacher_groups_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_teacher_groups_teachers_teacher_uid1",
                        column: x => x.teacher_uid1,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
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
                name: "enrollments",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    course_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    enrollment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enrollments", x => x.uid);
                    table.ForeignKey(
                        name: "fk_enrollments_courses_course_uid",
                        column: x => x.course_uid,
                        principalTable: "courses",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_enrollments_courses_course_uid1",
                        column: x => x.course_uid1,
                        principalTable: "courses",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_enrollments_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_enrollments_students_student_uid1",
                        column: x => x.student_uid1,
                        principalTable: "students",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "grades",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    assignment_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    value = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    graded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    assignment_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    graded_by_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grades", x => x.uid);
                    table.ForeignKey(
                        name: "fk_grades_assignments_assignment_uid",
                        column: x => x.assignment_uid,
                        principalTable: "assignments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_grades_assignments_assignment_uid1",
                        column: x => x.assignment_uid1,
                        principalTable: "assignments",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_grades_lessons_lesson_uid",
                        column: x => x.lesson_uid,
                        principalTable: "lessons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_grades_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grades_students_student_uid1",
                        column: x => x.student_uid1,
                        principalTable: "students",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_grades_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grades_subjects_subject_uid1",
                        column: x => x.subject_uid1,
                        principalTable: "subjects",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_grades_teachers_graded_by_uid",
                        column: x => x.graded_by_uid,
                        principalTable: "teachers",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_grades_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_grades_teachers_teacher_uid1",
                        column: x => x.teacher_uid1,
                        principalTable: "teachers",
                        principalColumn: "uid");
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
                name: "student_parents",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    relation_type = table.Column<int>(type: "integer", nullable: false),
                    is_emergency_contact = table.Column<bool>(type: "boolean", nullable: false),
                    has_access_to_grades = table.Column<bool>(type: "boolean", nullable: false),
                    has_access_to_attendance = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student_parents", x => x.uid);
                    table.ForeignKey(
                        name: "fk_student_parents_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_student_parents_users_parent_user_uid",
                        column: x => x.parent_user_uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "submissions",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    score = table.Column<double>(type: "double precision", nullable: true),
                    feedback = table.Column<string>(type: "text", nullable: false),
                    graded_by_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    graded_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    assignment_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_submissions", x => x.uid);
                    table.ForeignKey(
                        name: "fk_submissions_assignments_assignment_uid",
                        column: x => x.assignment_uid,
                        principalTable: "assignments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_submissions_assignments_assignment_uid1",
                        column: x => x.assignment_uid1,
                        principalTable: "assignments",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_submissions_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_submissions_students_student_uid1",
                        column: x => x.student_uid1,
                        principalTable: "students",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_submissions_teachers_graded_by_uid",
                        column: x => x.graded_by_uid,
                        principalTable: "teachers",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "grade_comments",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    author_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    grade_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grade_comments", x => x.uid);
                    table.ForeignKey(
                        name: "fk_grade_comments_grades_grade_uid",
                        column: x => x.grade_uid,
                        principalTable: "grades",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grade_comments_grades_grade_uid1",
                        column: x => x.grade_uid1,
                        principalTable: "grades",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "grade_revisions",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_value = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    new_value = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    previous_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    new_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    revision_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    grade_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grade_revisions", x => x.uid);
                    table.ForeignKey(
                        name: "fk_grade_revisions_grades_grade_uid",
                        column: x => x.grade_uid,
                        principalTable: "grades",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grade_revisions_grades_grade_uid1",
                        column: x => x.grade_uid1,
                        principalTable: "grades",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateIndex(
                name: "ix_assignments_course_uid",
                table: "assignments",
                column: "course_uid");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_course_uid1",
                table: "assignments",
                column: "course_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_lesson_uid",
                table: "assignments",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_lesson_uid1",
                table: "assignments",
                column: "lesson_uid1");

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
                name: "ix_auth_tokens_expires_at",
                table: "auth_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_auth_tokens_refresh_token",
                table: "auth_tokens",
                column: "refresh_token");

            migrationBuilder.CreateIndex(
                name: "ix_auth_tokens_token",
                table: "auth_tokens",
                column: "token");

            migrationBuilder.CreateIndex(
                name: "ix_courses_teacher_uid",
                table: "courses",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_courses_teacher_uid1",
                table: "courses",
                column: "teacher_uid1");

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
                name: "ix_enrollments_course_uid",
                table: "enrollments",
                column: "course_uid");

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_course_uid1",
                table: "enrollments",
                column: "course_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_student_uid",
                table: "enrollments",
                column: "student_uid");

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_student_uid1",
                table: "enrollments",
                column: "student_uid1");

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
                name: "ix_grade_comments_grade_uid",
                table: "grade_comments",
                column: "grade_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grade_comments_grade_uid1",
                table: "grade_comments",
                column: "grade_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_grade_revisions_grade_uid",
                table: "grade_revisions",
                column: "grade_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grade_revisions_grade_uid1",
                table: "grade_revisions",
                column: "grade_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_grades_assignment_uid",
                table: "grades",
                column: "assignment_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_assignment_uid1",
                table: "grades",
                column: "assignment_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_grades_graded_by_uid",
                table: "grades",
                column: "graded_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_lesson_uid",
                table: "grades",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_student_uid",
                table: "grades",
                column: "student_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_student_uid1",
                table: "grades",
                column: "student_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_grades_subject_uid",
                table: "grades",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_subject_uid1",
                table: "grades",
                column: "subject_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_grades_teacher_uid",
                table: "grades",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_teacher_uid1",
                table: "grades",
                column: "teacher_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_groups_code",
                table: "groups",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_groups_curator_uid",
                table: "groups",
                column: "curator_uid");

            migrationBuilder.CreateIndex(
                name: "ix_groups_department_uid",
                table: "groups",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_groups_start_date",
                table: "groups",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "ix_groups_status",
                table: "groups",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_groups_teacher_uid",
                table: "groups",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_groups_year",
                table: "groups",
                column: "year");

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
                name: "ix_lessons_group_uid",
                table: "lessons",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_module_uid",
                table: "lessons",
                column: "module_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_subject_uid",
                table: "lessons",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_teacher_uid",
                table: "lessons",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_modules_course_uid",
                table: "modules",
                column: "course_uid");

            migrationBuilder.CreateIndex(
                name: "ix_modules_course_uid1",
                table: "modules",
                column: "course_uid1");

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
                name: "ix_permissions_name",
                table: "permissions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_uid",
                table: "role_permissions",
                column: "permission_uid");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_uid1",
                table: "role_permissions",
                column: "permission_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_uid_permission_uid",
                table: "role_permissions",
                columns: new[] { "role_uid", "permission_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_uid1",
                table: "role_permissions",
                column: "role_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                table: "roles",
                column: "name",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "ix_student_parents_parent_user_uid",
                table: "student_parents",
                column: "parent_user_uid");

            migrationBuilder.CreateIndex(
                name: "ix_student_parents_student_uid_parent_user_uid",
                table: "student_parents",
                columns: new[] { "student_uid", "parent_user_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_email",
                table: "students",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_students_enrollment_date",
                table: "students",
                column: "enrollment_date");

            migrationBuilder.CreateIndex(
                name: "ix_students_group_uid",
                table: "students",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_students_status",
                table: "students",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_students_student_code",
                table: "students",
                column: "student_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_user_uid",
                table: "students",
                column: "user_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_user_uid1",
                table: "students",
                column: "user_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_code",
                table: "subjects",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subjects_department_uid",
                table: "subjects",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_department_uid1",
                table: "subjects",
                column: "department_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_is_active",
                table: "subjects",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_type",
                table: "subjects",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_assignment_uid",
                table: "submissions",
                column: "assignment_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_assignment_uid1",
                table: "submissions",
                column: "assignment_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_graded_by_uid",
                table: "submissions",
                column: "graded_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_student_uid",
                table: "submissions",
                column: "student_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_student_uid1",
                table: "submissions",
                column: "student_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_groups_group_uid",
                table: "teacher_groups",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_groups_group_uid1",
                table: "teacher_groups",
                column: "group_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_groups_subject_uid",
                table: "teacher_groups",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_groups_subject_uid1",
                table: "teacher_groups",
                column: "subject_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_groups_teacher_uid_group_uid_subject_uid",
                table: "teacher_groups",
                columns: new[] { "teacher_uid", "group_uid", "subject_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teacher_groups_teacher_uid1",
                table: "teacher_groups",
                column: "teacher_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_subjects_subject_uid",
                table: "teacher_subjects",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_subjects_subject_uid1",
                table: "teacher_subjects",
                column: "subject_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_subjects_teacher_uid_subject_uid",
                table: "teacher_subjects",
                columns: new[] { "teacher_uid", "subject_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teacher_subjects_teacher_uid1",
                table: "teacher_subjects",
                column: "teacher_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_department_uid",
                table: "teachers",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_employee_code",
                table: "teachers",
                column: "employee_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teachers_hire_date",
                table: "teachers",
                column: "hire_date");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_status",
                table: "teachers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_user_uid",
                table: "teachers",
                column: "user_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teachers_user_uid1",
                table: "teachers",
                column: "user_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_uid",
                table: "user_roles",
                column: "role_uid");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_uid_role_uid",
                table: "user_roles",
                columns: new[] { "user_uid", "role_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_uid1",
                table: "user_roles",
                column: "user_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_courses_course_uid",
                table: "assignments",
                column: "course_uid",
                principalTable: "courses",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_courses_course_uid1",
                table: "assignments",
                column: "course_uid1",
                principalTable: "courses",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_lessons_lesson_uid",
                table: "assignments",
                column: "lesson_uid",
                principalTable: "lessons",
                principalColumn: "uid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_lessons_lesson_uid1",
                table: "assignments",
                column: "lesson_uid1",
                principalTable: "lessons",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_attendances_lessons_lesson_uid",
                table: "attendances",
                column: "lesson_uid",
                principalTable: "lessons",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_attendances_students_student_uid",
                table: "attendances",
                column: "student_uid",
                principalTable: "students",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_attendances_teachers_checked_by_uid",
                table: "attendances",
                column: "checked_by_uid",
                principalTable: "teachers",
                principalColumn: "uid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_courses_teachers_teacher_uid",
                table: "courses",
                column: "teacher_uid",
                principalTable: "teachers",
                principalColumn: "uid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_courses_teachers_teacher_uid1",
                table: "courses",
                column: "teacher_uid1",
                principalTable: "teachers",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_departments_teachers_head_of_department_uid",
                table: "departments",
                column: "head_of_department_uid",
                principalTable: "teachers",
                principalColumn: "uid",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_departments_teachers_head_of_department_uid",
                table: "departments");

            migrationBuilder.DropTable(
                name: "attendances");

            migrationBuilder.DropTable(
                name: "auth_tokens");

            migrationBuilder.DropTable(
                name: "enrollments");

            migrationBuilder.DropTable(
                name: "file_records");

            migrationBuilder.DropTable(
                name: "grade_comments");

            migrationBuilder.DropTable(
                name: "grade_revisions");

            migrationBuilder.DropTable(
                name: "lesson_details");

            migrationBuilder.DropTable(
                name: "lesson_progress");

            migrationBuilder.DropTable(
                name: "notification_settings");

            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "schedules");

            migrationBuilder.DropTable(
                name: "student_parents");

            migrationBuilder.DropTable(
                name: "submissions");

            migrationBuilder.DropTable(
                name: "teacher_groups");

            migrationBuilder.DropTable(
                name: "teacher_subjects");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "grades");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "assignments");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "lessons");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "modules");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "teachers");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
