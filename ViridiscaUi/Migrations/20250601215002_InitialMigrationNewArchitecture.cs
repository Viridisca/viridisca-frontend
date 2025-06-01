using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ViridiscaUi.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrationNewArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academic_periods",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_academic_periods", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "curricula",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    total_credits = table.Column<int>(type: "integer", nullable: false),
                    duration_in_semesters = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_curricula", x => x.uid);
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
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_records", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "library_resources",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    author = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    isbn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    publisher = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    published_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    total_copies = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    available_copies = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_digital = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    digital_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_library_resources", x => x.uid);
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
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
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    profile_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persons", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    role_type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    is_email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    failed_login_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_failed_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.uid);
                    table.ForeignKey(
                        name: "fk_accounts_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "library_loans",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    borrower_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    return_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_returned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fine_amount = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_library_loans", x => x.uid);
                    table.ForeignKey(
                        name: "fk_library_loans_library_resources_resource_uid",
                        column: x => x.resource_uid,
                        principalTable: "library_resources",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_library_loans_persons_borrower_uid",
                        column: x => x.borrower_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_settings",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    email_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    push_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    sms_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    quiet_hours_start = table.Column<TimeSpan>(type: "interval", nullable: false),
                    quiet_hours_end = table.Column<TimeSpan>(type: "interval", nullable: false),
                    weekend_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    minimum_priority = table.Column<int>(type: "integer", nullable: false),
                    type_settings = table.Column<string>(type: "text", nullable: false),
                    category_settings = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_settings", x => x.uid);
                    table.ForeignKey(
                        name: "fk_notification_settings_persons_user_uid",
                        column: x => x.user_uid,
                        principalTable: "persons",
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
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    action_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_important = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    scheduled_for = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    repeat_interval = table.Column<TimeSpan>(type: "interval", nullable: true),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    metadata_json = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.uid);
                    table.ForeignKey(
                        name: "fk_notifications_persons_recipient_uid",
                        column: x => x.recipient_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_roles",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    role_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    assigned_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    context = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_roles", x => x.uid);
                    table.ForeignKey(
                        name: "fk_person_roles_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_person_roles_roles_role_uid",
                        column: x => x.role_uid,
                        principalTable: "roles",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
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
                name: "assignments",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_score = table.Column<double>(type: "double precision", nullable: false),
                    max_grade = table.Column<double>(type: "double precision", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    instructions = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    course_instance_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
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
                    checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    checked_by_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendances", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "course_instances",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_period_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    max_students = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    code = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    prerequisites = table.Column<string>(type: "text", nullable: false),
                    learning_outcomes = table.Column<string>(type: "text", nullable: false),
                    max_enrollments = table.Column<int>(type: "integer", nullable: false),
                    teacher_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_course_instances", x => x.uid);
                    table.ForeignKey(
                        name: "fk_course_instances_academic_periods_academic_period_uid",
                        column: x => x.academic_period_uid,
                        principalTable: "academic_periods",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exams",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    exam_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    max_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    academic_period_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exams", x => x.uid);
                    table.ForeignKey(
                        name: "fk_exams_academic_periods_academic_period_uid",
                        column: x => x.academic_period_uid,
                        principalTable: "academic_periods",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_exams_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schedule_slots",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    classroom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedule_slots", x => x.uid);
                    table.ForeignKey(
                        name: "fk_schedule_slots_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "curriculum_subjects",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    curriculum_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    semester = table.Column<int>(type: "integer", nullable: false),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_curriculum_subjects", x => x.uid);
                    table.ForeignKey(
                        name: "fk_curriculum_subjects_curricula_curriculum_uid",
                        column: x => x.curriculum_uid,
                        principalTable: "curricula",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
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
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
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
                    prerequisites = table.Column<string>(type: "text", nullable: false),
                    learning_outcomes = table.Column<string>(type: "text", nullable: false),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    lessons_per_week = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    department_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
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
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    hire_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    termination_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    qualification = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    specialization = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    salary = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    office_location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    working_hours = table.Column<string>(type: "text", nullable: false),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    department_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
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
                        name: "fk_teachers_departments_department_uid1",
                        column: x => x.department_uid1,
                        principalTable: "departments",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_teachers_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.uid);
                    table.ForeignKey(
                        name: "fk_groups_departments_department_uid",
                        column: x => x.department_uid,
                        principalTable: "departments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_groups_teachers_curator_uid",
                        column: x => x.curator_uid,
                        principalTable: "teachers",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    course_instance_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lessons", x => x.uid);
                    table.ForeignKey(
                        name: "fk_lessons_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lessons_course_instances_course_instance_uid1",
                        column: x => x.course_instance_uid1,
                        principalTable: "course_instances",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_lessons_groups_group_uid",
                        column: x => x.group_uid,
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_lessons_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_lessons_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    enrollment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    graduation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    emergency_contact_name = table.Column<string>(type: "text", nullable: false),
                    emergency_contact_phone = table.Column<string>(type: "text", nullable: false),
                    medical_information = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    gpa = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    curriculum_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    group_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_students", x => x.uid);
                    table.ForeignKey(
                        name: "fk_students_curricula_curriculum_uid",
                        column: x => x.curriculum_uid,
                        principalTable: "curricula",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_students_groups_group_uid",
                        column: x => x.group_uid,
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_students_groups_group_uid1",
                        column: x => x.group_uid1,
                        principalTable: "groups",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_students_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    enrolled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    final_grade = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enrollments", x => x.uid);
                    table.ForeignKey(
                        name: "fk_enrollments_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_enrollments_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_results",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    feedback = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    graded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_absent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_results", x => x.uid);
                    table.ForeignKey(
                        name: "fk_exam_results_exams_exam_uid",
                        column: x => x.exam_uid,
                        principalTable: "exams",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exam_results_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grades",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    assignment_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    graded_by_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    value = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    graded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grades", x => x.uid);
                    table.ForeignKey(
                        name: "fk_grades_assignments_assignment_uid",
                        column: x => x.assignment_uid,
                        principalTable: "assignments",
                        principalColumn: "uid");
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
                        name: "fk_grades_teachers_graded_by_uid",
                        column: x => x.graded_by_uid,
                        principalTable: "teachers",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_grades_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_progress",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "submissions",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    score = table.Column<double>(type: "numeric(5,2)", nullable: true),
                    feedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    graded_by_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    graded_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                        name: "fk_submissions_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_submissions_teachers_graded_by_uid",
                        column: x => x.graded_by_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "grade_comment",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    author_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    grade_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grade_comment", x => x.uid);
                    table.ForeignKey(
                        name: "fk_grade_comment_grades_grade_uid",
                        column: x => x.grade_uid,
                        principalTable: "grades",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grade_comment_grades_grade_uid1",
                        column: x => x.grade_uid1,
                        principalTable: "grades",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_grade_comment_persons_author_uid",
                        column: x => x.author_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grade_revision",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_value = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    new_value = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    previous_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    new_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    revision_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    grade_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grade_revision", x => x.uid);
                    table.ForeignKey(
                        name: "fk_grade_revision_grades_grade_uid",
                        column: x => x.grade_uid,
                        principalTable: "grades",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grade_revision_grades_grade_uid1",
                        column: x => x.grade_uid1,
                        principalTable: "grades",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_grade_revision_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "uid", "code", "created_at", "description", "head_of_department_uid", "is_active", "last_modified_at", "name" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "IT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кафедра информационных технологий и программирования", null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Информационные технологии" });

            migrationBuilder.InsertData(
                table: "persons",
                columns: new[] { "uid", "address", "created_at", "date_of_birth", "email", "first_name", "is_active", "last_modified_at", "last_name", "middle_name", "phone_number", "profile_image_url" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1989, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@viridisca.local", "Админ", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Системы", "Владимирович", "", "" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1984, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "teacher@viridisca.local", "Преподаватель", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Тестовый", "Иванович", "+7 (900) 123-45-67", "" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2004, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "student@viridisca.local", "Студент", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Тестовый", "Петрович", "+7 (900) 987-65-43", "" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "uid", "created_at", "description", "last_modified_at", "name", "role_type" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Системный администратор", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SystemAdmin", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Преподаватель", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teacher", 0 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Студент", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Student", 0 }
                });

            migrationBuilder.InsertData(
                table: "accounts",
                columns: new[] { "uid", "created_at", "is_active", "is_email_confirmed", "last_failed_login_at", "last_login_at", "last_modified_at", "locked_until", "password_hash", "person_uid", "username" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$2a$11$GjZru83/8zIBf7TRU5keb.J1s/puDucqcVdPVqvXG02gMcw3F.qZm", new Guid("44444444-4444-4444-4444-444444444444"), "admin" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$2a$11$ybAXNdR88fN3iscBh5CZDeO1YKI4JrHeLANB49AYIY/atCD9XWvWm", new Guid("55555555-5555-5555-5555-555555555555"), "teacher" },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$2a$11$goaePviZrvAmpoMYoiodQuuXeQu/50rgF6omjV3xqee8lA9AihvRK", new Guid("66666666-6666-6666-6666-666666666666"), "student" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_academic_periods_academic_year_type",
                table: "academic_periods",
                columns: new[] { "academic_year", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_accounts_person_uid",
                table: "accounts",
                column: "person_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_username",
                table: "accounts",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignments_course_instance_uid",
                table: "assignments",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_course_instance_uid1",
                table: "assignments",
                column: "course_instance_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_lesson_uid",
                table: "assignments",
                column: "lesson_uid");

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
                name: "ix_course_instances_academic_period_uid",
                table: "course_instances",
                column: "academic_period_uid");

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_group_uid",
                table: "course_instances",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_subject_uid_group_uid_academic_period_uid",
                table: "course_instances",
                columns: new[] { "subject_uid", "group_uid", "academic_period_uid" });

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_teacher_uid",
                table: "course_instances",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_teacher_uid1",
                table: "course_instances",
                column: "teacher_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_curriculum_subjects_curriculum_uid_subject_uid",
                table: "curriculum_subjects",
                columns: new[] { "curriculum_uid", "subject_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_curriculum_subjects_subject_uid",
                table: "curriculum_subjects",
                column: "subject_uid");

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
                name: "ix_enrollments_course_instance_uid",
                table: "enrollments",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_student_uid_course_instance_uid",
                table: "enrollments",
                columns: new[] { "student_uid", "course_instance_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exam_results_exam_uid_student_uid",
                table: "exam_results",
                columns: new[] { "exam_uid", "student_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exam_results_student_uid",
                table: "exam_results",
                column: "student_uid");

            migrationBuilder.CreateIndex(
                name: "ix_exams_academic_period_uid",
                table: "exams",
                column: "academic_period_uid");

            migrationBuilder.CreateIndex(
                name: "ix_exams_course_instance_uid",
                table: "exams",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_exams_exam_date",
                table: "exams",
                column: "exam_date");

            migrationBuilder.CreateIndex(
                name: "ix_exams_is_published",
                table: "exams",
                column: "is_published");

            migrationBuilder.CreateIndex(
                name: "ix_file_records_content_type",
                table: "file_records",
                column: "content_type");

            migrationBuilder.CreateIndex(
                name: "ix_file_records_entity_type_entity_uid",
                table: "file_records",
                columns: new[] { "entity_type", "entity_uid" });

            migrationBuilder.CreateIndex(
                name: "ix_file_records_uploaded_by_uid",
                table: "file_records",
                column: "uploaded_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grade_comment_author_uid",
                table: "grade_comment",
                column: "author_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grade_comment_grade_uid",
                table: "grade_comment",
                column: "grade_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grade_comment_grade_uid1",
                table: "grade_comment",
                column: "grade_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_grade_revision_grade_uid",
                table: "grade_revision",
                column: "grade_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grade_revision_grade_uid1",
                table: "grade_revision",
                column: "grade_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_grade_revision_teacher_uid",
                table: "grade_revision",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_assignment_uid",
                table: "grades",
                column: "assignment_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_graded_by_uid",
                table: "grades",
                column: "graded_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_is_published",
                table: "grades",
                column: "is_published");

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
                name: "ix_grades_teacher_uid",
                table: "grades",
                column: "teacher_uid");

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
                name: "ix_groups_status",
                table: "groups",
                column: "status");

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
                name: "ix_lessons_course_instance_uid",
                table: "lessons",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_course_instance_uid1",
                table: "lessons",
                column: "course_instance_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_group_uid",
                table: "lessons",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_subject_uid",
                table: "lessons",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_teacher_uid",
                table: "lessons",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_library_loans_borrower_uid",
                table: "library_loans",
                column: "borrower_uid");

            migrationBuilder.CreateIndex(
                name: "ix_library_loans_due_date",
                table: "library_loans",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "ix_library_loans_is_returned",
                table: "library_loans",
                column: "is_returned");

            migrationBuilder.CreateIndex(
                name: "ix_library_loans_resource_uid",
                table: "library_loans",
                column: "resource_uid");

            migrationBuilder.CreateIndex(
                name: "ix_library_resources_isbn",
                table: "library_resources",
                column: "isbn");

            migrationBuilder.CreateIndex(
                name: "ix_library_resources_title",
                table: "library_resources",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "ix_library_resources_type",
                table: "library_resources",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_notification_settings_user_uid",
                table: "notification_settings",
                column: "user_uid");

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
                name: "ix_person_roles_person_uid_role_uid_context",
                table: "person_roles",
                columns: new[] { "person_uid", "role_uid", "context" });

            migrationBuilder.CreateIndex(
                name: "ix_person_roles_role_uid",
                table: "person_roles",
                column: "role_uid");

            migrationBuilder.CreateIndex(
                name: "ix_persons_email",
                table: "persons",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_persons_phone_number",
                table: "persons",
                column: "phone_number");

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
                name: "ix_schedule_slots_course_instance_uid_day_of_week_start_time",
                table: "schedule_slots",
                columns: new[] { "course_instance_uid", "day_of_week", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_students_curriculum_uid",
                table: "students",
                column: "curriculum_uid");

            migrationBuilder.CreateIndex(
                name: "ix_students_group_uid",
                table: "students",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_students_group_uid1",
                table: "students",
                column: "group_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_students_person_uid",
                table: "students",
                column: "person_uid",
                unique: true);

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
                name: "ix_subjects_type",
                table: "subjects",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_assignment_uid",
                table: "submissions",
                column: "assignment_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_graded_by_uid",
                table: "submissions",
                column: "graded_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_student_uid_assignment_uid",
                table: "submissions",
                columns: new[] { "student_uid", "assignment_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teachers_department_uid",
                table: "teachers",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_department_uid1",
                table: "teachers",
                column: "department_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_employee_code",
                table: "teachers",
                column: "employee_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teachers_person_uid",
                table: "teachers",
                column: "person_uid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_course_instances_course_instance_uid",
                table: "assignments",
                column: "course_instance_uid",
                principalTable: "course_instances",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_course_instances_course_instance_uid1",
                table: "assignments",
                column: "course_instance_uid1",
                principalTable: "course_instances",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_lessons_lesson_uid",
                table: "assignments",
                column: "lesson_uid",
                principalTable: "lessons",
                principalColumn: "uid",
                onDelete: ReferentialAction.SetNull);

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
                name: "fk_course_instances_groups_group_uid",
                table: "course_instances",
                column: "group_uid",
                principalTable: "groups",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_course_instances_subjects_subject_uid",
                table: "course_instances",
                column: "subject_uid",
                principalTable: "subjects",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_course_instances_teachers_teacher_uid",
                table: "course_instances",
                column: "teacher_uid",
                principalTable: "teachers",
                principalColumn: "uid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_course_instances_teachers_teacher_uid1",
                table: "course_instances",
                column: "teacher_uid1",
                principalTable: "teachers",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_curriculum_subjects_subjects_subject_uid",
                table: "curriculum_subjects",
                column: "subject_uid",
                principalTable: "subjects",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_departments_teachers_head_of_department_uid",
                table: "departments",
                column: "head_of_department_uid",
                principalTable: "teachers",
                principalColumn: "uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_teachers_persons_person_uid",
                table: "teachers");

            migrationBuilder.DropForeignKey(
                name: "fk_departments_teachers_head_of_department_uid",
                table: "departments");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "attendances");

            migrationBuilder.DropTable(
                name: "curriculum_subjects");

            migrationBuilder.DropTable(
                name: "enrollments");

            migrationBuilder.DropTable(
                name: "exam_results");

            migrationBuilder.DropTable(
                name: "file_records");

            migrationBuilder.DropTable(
                name: "grade_comment");

            migrationBuilder.DropTable(
                name: "grade_revision");

            migrationBuilder.DropTable(
                name: "lesson_progress");

            migrationBuilder.DropTable(
                name: "library_loans");

            migrationBuilder.DropTable(
                name: "notification_settings");

            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "person_roles");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "schedule_slots");

            migrationBuilder.DropTable(
                name: "submissions");

            migrationBuilder.DropTable(
                name: "exams");

            migrationBuilder.DropTable(
                name: "grades");

            migrationBuilder.DropTable(
                name: "library_resources");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "assignments");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "lessons");

            migrationBuilder.DropTable(
                name: "curricula");

            migrationBuilder.DropTable(
                name: "course_instances");

            migrationBuilder.DropTable(
                name: "academic_periods");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "persons");

            migrationBuilder.DropTable(
                name: "teachers");

            migrationBuilder.DropTable(
                name: "departments");
        }
    }
}
