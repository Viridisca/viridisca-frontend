using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ViridiscaUi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academic_periods",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_academic_periods", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "library_resources",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    author = table.Column<string>(type: "text", nullable: true),
                    isbn = table.Column<string>(type: "text", nullable: true),
                    publisher = table.Column<string>(type: "text", nullable: true),
                    published_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resource_type = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    location = table.Column<string>(type: "text", nullable: true),
                    total_copies = table.Column<int>(type: "integer", nullable: false),
                    available_copies = table.Column<int>(type: "integer", nullable: false),
                    is_digital = table.Column<bool>(type: "boolean", nullable: false),
                    digital_url = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    title_template = table.Column<string>(type: "text", nullable: false),
                    message_template = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    parameters_json = table.Column<string>(type: "text", nullable: true),
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
                    name = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    profile_image_url = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    name = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    role_type = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    data_type = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_settings", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    is_email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    failed_login_attempts = table.Column<int>(type: "integer", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_failed_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "file_records",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    original_file_name = table.Column<string>(type: "text", nullable: false),
                    stored_file_name = table.Column<string>(type: "text", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: true),
                    entity_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_records", x => x.uid);
                    table.ForeignKey(
                        name: "fk_file_records_persons_uploaded_by_uid",
                        column: x => x.uploaded_by_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "library_loans",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    loaned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    returned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fine_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                        name: "fk_library_loans_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_settings",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    email_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    sms_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    push_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    minimum_priority = table.Column<int>(type: "integer", nullable: false),
                    type_settings_json = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_settings", x => x.uid);
                    table.ForeignKey(
                        name: "fk_notification_settings_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    template_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    action_url = table.Column<string>(type: "text", nullable: true),
                    metadata_json = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.uid);
                    table.ForeignKey(
                        name: "fk_notifications_notification_templates_template_uid",
                        column: x => x.template_uid,
                        principalTable: "notification_templates",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_notifications_persons_person_uid",
                        column: x => x.person_uid,
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
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    context_entity_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    context_entity_type = table.Column<string>(type: "text", nullable: true),
                    context = table.Column<string>(type: "text", nullable: true),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    role_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.role_uid, x.permission_uid });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_uid",
                        column: x => x.permission_uid,
                        principalTable: "permissions",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_uid",
                        column: x => x.role_uid,
                        principalTable: "roles",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assignments",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_score = table.Column<double>(type: "double precision", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    instructions = table.Column<string>(type: "text", nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    notes = table.Column<string>(type: "text", nullable: true),
                    checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    checked_by_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    lesson_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_enrollments = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    academic_period_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    exam_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    location = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    max_score = table.Column<decimal>(type: "numeric", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    course_instance_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exams", x => x.uid);
                    table.ForeignKey(
                        name: "fk_exams_academic_periods_academic_period_uid",
                        column: x => x.academic_period_uid,
                        principalTable: "academic_periods",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exams_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exams_course_instances_course_instance_uid1",
                        column: x => x.course_instance_uid1,
                        principalTable: "course_instances",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "schedule_slots",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    room = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "curricula",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    total_credits = table.Column<int>(type: "integer", nullable: false),
                    duration_semesters = table.Column<int>(type: "integer", nullable: false),
                    duration_months = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_curricula", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "curriculum_subjects",
                columns: table => new
                {
                    curriculum_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    semester = table.Column<int>(type: "integer", nullable: false),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_curriculum_subjects", x => new { x.curriculum_uid, x.subject_uid });
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
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
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
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "text", nullable: true),
                    prerequisites = table.Column<string>(type: "text", nullable: true),
                    learning_outcomes = table.Column<string>(type: "text", nullable: true),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    lessons_per_week = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "teachers",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_code = table.Column<string>(type: "text", nullable: false),
                    hire_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    termination_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    qualification = table.Column<string>(type: "text", nullable: false),
                    specialization = table.Column<string>(type: "text", nullable: true),
                    salary = table.Column<decimal>(type: "numeric", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    office_location = table.Column<string>(type: "text", nullable: true),
                    working_hours = table.Column<string>(type: "text", nullable: true),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    academic_degree = table.Column<string>(type: "text", nullable: true),
                    academic_title = table.Column<string>(type: "text", nullable: true),
                    hourly_rate = table.Column<decimal>(type: "numeric", nullable: true),
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
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    year = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_students = table.Column<int>(type: "integer", nullable: false),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    curator_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    curator_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    curriculum_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.uid);
                    table.ForeignKey(
                        name: "fk_groups_curricula_curriculum_uid",
                        column: x => x.curriculum_uid,
                        principalTable: "curricula",
                        principalColumn: "uid");
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
                        name: "fk_groups_teachers_curator_uid1",
                        column: x => x.curator_uid1,
                        principalTable: "teachers",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_code = table.Column<string>(type: "text", nullable: false),
                    enrollment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    graduation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    gpa = table.Column<decimal>(type: "numeric", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    curriculum_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    curriculum_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                        name: "fk_students_curricula_curriculum_uid1",
                        column: x => x.curriculum_uid1,
                        principalTable: "curricula",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_students_groups_group_uid",
                        column: x => x.group_uid,
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
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
                    status = table.Column<int>(type: "integer", nullable: false),
                    final_grade = table.Column<decimal>(type: "numeric", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    score = table.Column<decimal>(type: "numeric", nullable: false),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    graded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_absent = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    table.ForeignKey(
                        name: "fk_exam_results_students_student_uid1",
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
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    exam_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    value = table.Column<decimal>(type: "numeric", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_value = table.Column<decimal>(type: "numeric", nullable: true),
                    weight = table.Column<decimal>(type: "numeric", nullable: false),
                    teacher_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
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
                        name: "fk_grades_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grades_exams_exam_uid",
                        column: x => x.exam_uid,
                        principalTable: "exams",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_grades_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grades_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_grades_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_grades_teachers_teacher_uid1",
                        column: x => x.teacher_uid1,
                        principalTable: "teachers",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "lesson_progresses",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    completion_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    time_spent_minutes = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: true),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    lesson_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_progresses", x => x.uid);
                    table.ForeignKey(
                        name: "fk_lesson_progresses_lessons_lesson_uid",
                        column: x => x.lesson_uid,
                        principalTable: "lessons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_progresses_lessons_lesson_uid1",
                        column: x => x.lesson_uid1,
                        principalTable: "lessons",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_lesson_progresses_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_progresses_students_student_uid1",
                        column: x => x.student_uid1,
                        principalTable: "students",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "submissions",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    submission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    score = table.Column<double>(type: "double precision", nullable: true),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    graded_by_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    graded_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    grade_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
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
                        name: "fk_submissions_grades_grade_uid",
                        column: x => x.grade_uid,
                        principalTable: "grades",
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

            migrationBuilder.InsertData(
                table: "academic_periods",
                columns: new[] { "uid", "academic_year", "code", "created_at", "description", "end_date", "is_active", "is_current", "last_modified_at", "name", "start_date", "type" },
                values: new object[,]
                {
                    { new Guid("aa111111-1111-1111-1111-111111111111"), 2024, "FALL2024", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Осенний семестр 2024", new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { new Guid("aa222222-2222-2222-2222-222222222222"), 2024, "SPRING2025", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc), false, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Весенний семестр 2025", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "uid", "code", "created_at", "description", "head_of_department_uid", "is_active", "last_modified_at", "name" },
                values: new object[,]
                {
                    { new Guid("d1111111-1111-1111-1111-111111111111"), "IT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кафедра информационных технологий и программирования", null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Информационные технологии" },
                    { new Guid("d2222222-2222-2222-2222-222222222222"), "MATH", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кафедра математики и статистики", null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Математика" },
                    { new Guid("d3333333-3333-3333-3333-333333333333"), "PHYS", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кафедра физики и естественных наук", null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Физика" },
                    { new Guid("d4444444-4444-4444-4444-444444444444"), "LANG", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кафедра иностранных языков", null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Иностранные языки" },
                    { new Guid("d5555555-5555-5555-5555-555555555555"), "ECON", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кафедра экономики и менеджмента", null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Экономика" }
                });

            migrationBuilder.InsertData(
                table: "notification_templates",
                columns: new[] { "uid", "category", "created_at", "description", "is_active", "last_modified_at", "message_template", "name", "parameters_json", "priority", "title_template", "type" },
                values: new object[,]
                {
                    { new Guid("b1111111-1111-1111-1111-111111111111"), "Welcome", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Здравствуйте, {StudentName}! Добро пожаловать в систему управления обучением Viridisca LMS. Ваш логин: {Username}", "WelcomeStudent", null, 1, "Добро пожаловать в Viridisca LMS!", 0 },
                    { new Guid("b2222222-2222-2222-2222-222222222222"), "Academic", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Здравствуйте, {StudentName}! По предмету '{CourseName}' выставлена новая оценка: {Grade}", "GradePublished", null, 1, "Новая оценка", 0 },
                    { new Guid("b3333333-3333-3333-3333-333333333333"), "Reminder", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Здравствуйте, {StudentName}! Напоминаем, что задание '{AssignmentName}' должно быть сдано до {DueDate}", "AssignmentDue", null, 1, "Напоминание о задании", 0 }
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "uid", "created_at", "description", "display_name", "last_modified_at", "name" },
                values: new object[,]
                {
                    { new Guid("10000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Неограниченный доступ ко всем функциям", "Полный доступ к системе", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SystemAdmin.FullAccess" },
                    { new Guid("10000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Просмотр списка пользователей", "Просмотр пользователей", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users.View" },
                    { new Guid("10000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Создание новых пользователей", "Создание пользователей", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users.Create" },
                    { new Guid("10000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Изменение данных пользователей", "Редактирование пользователей", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users.Edit" },
                    { new Guid("10000005-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Удаление пользователей из системы", "Удаление пользователей", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users.Delete" },
                    { new Guid("20000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Просмотр списка студентов", "Просмотр студентов", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Students.View" },
                    { new Guid("20000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Регистрация новых студентов", "Создание студентов", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Students.Create" },
                    { new Guid("20000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Изменение данных студентов", "Редактирование студентов", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Students.Edit" },
                    { new Guid("20000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Удаление студентов из системы", "Удаление студентов", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Students.Delete" },
                    { new Guid("30000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Просмотр списка преподавателей", "Просмотр преподавателей", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teachers.View" },
                    { new Guid("30000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Регистрация новых преподавателей", "Создание преподавателей", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teachers.Create" },
                    { new Guid("30000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Изменение данных преподавателей", "Редактирование преподавателей", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teachers.Edit" },
                    { new Guid("30000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Удаление преподавателей из системы", "Удаление преподавателей", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teachers.Delete" },
                    { new Guid("40000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Просмотр списка курсов", "Просмотр курсов", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Courses.View" },
                    { new Guid("40000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Создание новых курсов", "Создание курсов", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Courses.Create" },
                    { new Guid("40000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Изменение курсов", "Редактирование курсов", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Courses.Edit" },
                    { new Guid("40000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Удаление курсов", "Удаление курсов", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Courses.Delete" },
                    { new Guid("50000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Просмотр оценок студентов", "Просмотр оценок", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grades.View" },
                    { new Guid("50000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Создание новых оценок", "Выставление оценок", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grades.Create" },
                    { new Guid("50000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Изменение оценок", "Редактирование оценок", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grades.Edit" },
                    { new Guid("50000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Удаление оценок", "Удаление оценок", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grades.Delete" },
                    { new Guid("60000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Просмотр библиотечных ресурсов", "Просмотр библиотеки", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Library.View" },
                    { new Guid("60000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Управление библиотечными ресурсами", "Управление библиотекой", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Library.Manage" }
                });

            migrationBuilder.InsertData(
                table: "persons",
                columns: new[] { "uid", "address", "created_at", "date_of_birth", "email", "first_name", "is_active", "last_modified_at", "last_name", "middle_name", "phone", "phone_number", "profile_image_url" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1985, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), "a.petrova@viridisca.edu", "Анна", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Петрова", "Сергеевна", "+7 (999) 234-56-78", "+7 (999) 234-56-78", null },
                    { new Guid("22222222-0000-0000-0000-000000000001"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2003, 8, 20, 0, 0, 0, 0, DateTimeKind.Utc), "i.ivanov@student.viridisca.edu", "Иван", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Иванов", "Петрович", "+7 (999) 345-67-89", "+7 (999) 345-67-89", null },
                    { new Guid("33333333-0000-0000-0000-000000000002"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2003, 12, 10, 0, 0, 0, 0, DateTimeKind.Utc), "m.sidorova@student.viridisca.edu", "Мария", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Сидорова", "Александровна", "+7 (999) 456-78-90", "+7 (999) 456-78-90", null },
                    { new Guid("aaaabbbb-0000-0000-0000-000000000001"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1980, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@viridisca.edu", "Системный", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Администратор", null, "+7 (999) 123-45-67", "+7 (999) 123-45-67", null }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "uid", "created_at", "description", "display_name", "last_modified_at", "name", "role_type" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Полный доступ ко всем функциям системы", "Системный администратор", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SystemAdmin", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Управление академическими процессами", "Начальник учебной части", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AcademicAffairsHead", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Управление кафедрой и её ресурсами", "Заведующий кафедрой", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DepartmentHead", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ведение занятий и оценивание студентов", "Преподаватель", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teacher", null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Помощь в ведении занятий", "Ассистент преподавателя", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AssistantTeacher", null },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Обучение в системе", "Студент", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Student", null },
                    { new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Просмотр успеваемости ребёнка", "Родитель", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Parent", null },
                    { new Guid("88888888-8888-8888-8888-888888888888"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Управление библиотечными ресурсами", "Библиотекарь", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Librarian", null },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Техническая поддержка системы", "IT поддержка", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ITSupport", null },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Финансовые операции", "Бухгалтер", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Accountant", null }
                });

            migrationBuilder.InsertData(
                table: "system_settings",
                columns: new[] { "uid", "category", "created_at", "data_type", "description", "is_system", "key", "last_modified_at", "value" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", "Название системы", false, "System.Name", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Viridisca LMS" },
                    { new Guid("a2222222-2222-2222-2222-222222222222"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", "Версия системы", false, "System.Version", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1.0.0" },
                    { new Guid("a3333333-3333-3333-3333-333333333333"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", "Количество кредитов по умолчанию для предмета", false, "Academic.DefaultCredits", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "3" },
                    { new Guid("a4444444-4444-4444-4444-444444444444"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", "Максимальная оценка в системе", false, "Academic.MaxGrade", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "5.0" },
                    { new Guid("a5555555-5555-5555-5555-555555555555"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", "Минимальная проходная оценка", false, "Academic.MinPassingGrade", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2.5" },
                    { new Guid("a6666666-6666-6666-6666-666666666666"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", "Максимальный срок займа библиотечного ресурса (дни)", false, "Library.MaxLoanDays", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "30" },
                    { new Guid("a7777777-7777-7777-7777-777777777777"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", "Включены ли email уведомления", false, "Notification.EmailEnabled", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "true" }
                });

            migrationBuilder.InsertData(
                table: "accounts",
                columns: new[] { "uid", "created_at", "failed_login_attempts", "is_active", "is_email_confirmed", "is_locked", "last_failed_login_at", "last_login_at", "last_modified_at", "locked_until", "password_hash", "person_uid", "username" },
                values: new object[,]
                {
                    { new Guid("aaaabbbb-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, true, true, false, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$2a$11$k0ODYBCbIng48AKxSkhd8..4baR4aTkTME4d0hgiDHjMvmzlJteBu", new Guid("aaaabbbb-0000-0000-0000-000000000001"), "admin" },
                    { new Guid("bbbbbbbb-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, true, true, false, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$2a$11$h5g50RMYfnJyebkKM.g4GerEoAaAP2Lwip/PrsXlSd9MybB4600GC", new Guid("11111111-0000-0000-0000-000000000001"), "a.petrova" },
                    { new Guid("cccccccc-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, true, true, false, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$2a$11$Z7iP6z0MgMUyCWa6BEeL5.6ij6XyLea7l8V4t3O8QoDJXrT9gsgEe", new Guid("22222222-0000-0000-0000-000000000001"), "i.ivanov" },
                    { new Guid("dddddddd-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, true, true, false, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$2a$11$k.ystkXxXPTB4yZy6mGvXOuVVLLk9al9UFRftBG6GDBC6K2g/tYEe", new Guid("33333333-0000-0000-0000-000000000002"), "m.sidorova" }
                });

            migrationBuilder.InsertData(
                table: "curricula",
                columns: new[] { "uid", "code", "created_at", "department_uid", "description", "duration_months", "duration_semesters", "is_active", "last_modified_at", "name", "total_credits", "valid_from", "valid_to" },
                values: new object[] { new Guid("eeeeeeee-0000-0000-0000-000000000001"), "IT-2021", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("d1111111-1111-1111-1111-111111111111"), "Учебный план по направлению Информационные технологии", 48, 0, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Информационные технологии 2021", 240, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null });

            migrationBuilder.InsertData(
                table: "groups",
                columns: new[] { "uid", "code", "created_at", "curator_uid", "curator_uid1", "curriculum_uid", "department_uid", "description", "end_date", "is_active", "last_modified_at", "max_students", "name", "start_date", "status", "year" },
                values: new object[] { new Guid("dddddddd-0000-0000-0000-000000000001"), "IT-21-1", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("d1111111-1111-1111-1111-111111111111"), null, null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "Информационные технологии 2021, группа 1", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, 0 });

            migrationBuilder.InsertData(
                table: "person_roles",
                columns: new[] { "uid", "assigned_at", "assigned_by", "context", "context_entity_type", "context_entity_uid", "created_at", "expires_at", "is_active", "last_modified_at", "person_uid", "role_uid" },
                values: new object[,]
                {
                    { new Guid("aaaabbbb-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaabbbb-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("bbbbbbbb-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("cccccccc-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-0000-0000-0000-000000000001"), new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("dddddddd-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-0000-0000-0000-000000000002"), new Guid("66666666-6666-6666-6666-666666666666") }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_uid", "role_uid", "created_at", "last_modified_at", "uid" },
                values: new object[,]
                {
                    { new Guid("10000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("10000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("10000003-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("10000004-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("10000005-0000-0000-0000-000000000005"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("20000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("20000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("20000003-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("20000004-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("30000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("30000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("30000003-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("30000004-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("40000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("40000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("40000003-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("40000004-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("50000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("50000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("50000003-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("50000004-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("60000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("60000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("20000001-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("40000001-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("50000001-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("50000002-0000-0000-0000-000000000002"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("50000003-0000-0000-0000-000000000003"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("50000004-0000-0000-0000-000000000004"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("60000001-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("40000001-0000-0000-0000-000000000001"), new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("50000001-0000-0000-0000-000000000001"), new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("60000001-0000-0000-0000-000000000001"), new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("00000000-0000-0000-0000-000000000000") }
                });

            migrationBuilder.InsertData(
                table: "subjects",
                columns: new[] { "uid", "category", "code", "created_at", "credits", "department_uid", "description", "is_active", "last_modified_at", "learning_outcomes", "lessons_per_week", "name", "prerequisites", "type" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000001"), null, "CS101", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new Guid("d1111111-1111-1111-1111-111111111111"), "Введение в программирование на C#", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, "Основы программирования", null, 1 },
                    { new Guid("bbbbbbbb-0000-0000-0000-000000000002"), null, "MATH201", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, new Guid("d2222222-2222-2222-2222-222222222222"), "Математический анализ и линейная алгебра", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, "Высшая математика", null, 1 },
                    { new Guid("cccccccc-0000-0000-0000-000000000003"), null, "ENG101", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, new Guid("d4444444-4444-4444-4444-444444444444"), "Базовый курс английского языка", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, "Английский язык", null, 2 }
                });

            migrationBuilder.InsertData(
                table: "teachers",
                columns: new[] { "uid", "academic_degree", "academic_title", "created_at", "department_uid", "employee_code", "hire_date", "hourly_rate", "is_active", "last_modified_at", "office_location", "person_uid", "qualification", "salary", "specialization", "termination_date", "working_hours" },
                values: new object[] { new Guid("bbbbbbbb-3333-3333-3333-333333333333"), null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("d1111111-1111-1111-1111-111111111111"), "T001", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("11111111-0000-0000-0000-000000000001"), "Кандидат технических наук", 75000m, null, null, null });

            migrationBuilder.InsertData(
                table: "curriculum_subjects",
                columns: new[] { "curriculum_uid", "subject_uid", "created_at", "credits", "is_mandatory", "is_required", "last_modified_at", "semester", "uid" },
                values: new object[,]
                {
                    { new Guid("eeeeeeee-0000-0000-0000-000000000001"), new Guid("aaaaaaaa-0000-0000-0000-000000000001"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, true, true, null, 1, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("eeeeeeee-0000-0000-0000-000000000001"), new Guid("bbbbbbbb-0000-0000-0000-000000000002"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, true, true, null, 1, new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("eeeeeeee-0000-0000-0000-000000000001"), new Guid("cccccccc-0000-0000-0000-000000000003"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, false, false, null, 2, new Guid("00000000-0000-0000-0000-000000000000") }
                });

            migrationBuilder.InsertData(
                table: "students",
                columns: new[] { "uid", "created_at", "curriculum_uid", "curriculum_uid1", "enrollment_date", "gpa", "graduation_date", "group_uid", "last_modified_at", "person_uid", "status", "student_code" },
                values: new object[,]
                {
                    { new Guid("aaaabbbb-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("eeeeeeee-0000-0000-0000-000000000001"), null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3.8m, null, new Guid("dddddddd-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-0000-0000-0000-000000000002"), 1, "S2021002" },
                    { new Guid("ffffffff-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("eeeeeeee-0000-0000-0000-000000000001"), null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4.2m, null, new Guid("dddddddd-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-0000-0000-0000-000000000001"), 1, "S2021001" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_academic_periods_academic_year_type",
                table: "academic_periods",
                columns: new[] { "academic_year", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_academic_periods_code",
                table: "academic_periods",
                column: "code",
                unique: true);

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
                name: "ix_assignments_lesson_uid",
                table: "assignments",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_attendances_lesson_uid",
                table: "attendances",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_attendances_lesson_uid1",
                table: "attendances",
                column: "lesson_uid1");

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
                name: "ix_curricula_code",
                table: "curricula",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_curricula_department_uid",
                table: "curricula",
                column: "department_uid");

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
                name: "ix_exam_results_student_uid1",
                table: "exam_results",
                column: "student_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_exams_academic_period_uid",
                table: "exams",
                column: "academic_period_uid");

            migrationBuilder.CreateIndex(
                name: "ix_exams_course_instance_uid",
                table: "exams",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_exams_course_instance_uid1",
                table: "exams",
                column: "course_instance_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_file_records_file_path",
                table: "file_records",
                column: "file_path");

            migrationBuilder.CreateIndex(
                name: "ix_file_records_uploaded_by_uid",
                table: "file_records",
                column: "uploaded_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_assignment_uid",
                table: "grades",
                column: "assignment_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_course_instance_uid",
                table: "grades",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_exam_uid",
                table: "grades",
                column: "exam_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_student_uid",
                table: "grades",
                column: "student_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_subject_uid",
                table: "grades",
                column: "subject_uid");

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
                name: "ix_groups_curator_uid1",
                table: "groups",
                column: "curator_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_groups_curriculum_uid",
                table: "groups",
                column: "curriculum_uid");

            migrationBuilder.CreateIndex(
                name: "ix_groups_department_uid",
                table: "groups",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progresses_lesson_uid",
                table: "lesson_progresses",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progresses_lesson_uid1",
                table: "lesson_progresses",
                column: "lesson_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progresses_student_uid_lesson_uid",
                table: "lesson_progresses",
                columns: new[] { "student_uid", "lesson_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progresses_student_uid1",
                table: "lesson_progresses",
                column: "student_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_course_instance_uid",
                table: "lessons",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_library_loans_person_uid",
                table: "library_loans",
                column: "person_uid");

            migrationBuilder.CreateIndex(
                name: "ix_library_loans_resource_uid",
                table: "library_loans",
                column: "resource_uid");

            migrationBuilder.CreateIndex(
                name: "ix_library_resources_isbn",
                table: "library_resources",
                column: "isbn");

            migrationBuilder.CreateIndex(
                name: "ix_notification_settings_person_uid",
                table: "notification_settings",
                column: "person_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notification_templates_name",
                table: "notification_templates",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_person_uid_created_at",
                table: "notifications",
                columns: new[] { "person_uid", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_template_uid",
                table: "notifications",
                column: "template_uid");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_name",
                table: "permissions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_person_roles_person_uid_role_uid_context_entity_uid_context",
                table: "person_roles",
                columns: new[] { "person_uid", "role_uid", "context_entity_uid", "context_entity_type" });

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
                name: "ix_role_permissions_permission_uid",
                table: "role_permissions",
                column: "permission_uid");

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
                name: "ix_students_curriculum_uid1",
                table: "students",
                column: "curriculum_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_students_group_uid",
                table: "students",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_students_person_uid",
                table: "students",
                column: "person_uid",
                unique: true);

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
                name: "ix_submissions_assignment_uid",
                table: "submissions",
                column: "assignment_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_grade_uid",
                table: "submissions",
                column: "grade_uid");

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
                name: "ix_system_settings_key",
                table: "system_settings",
                column: "key",
                unique: true);

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
                name: "fk_assignments_lessons_lesson_uid",
                table: "assignments",
                column: "lesson_uid",
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
                name: "fk_attendances_lessons_lesson_uid1",
                table: "attendances",
                column: "lesson_uid1",
                principalTable: "lessons",
                principalColumn: "uid");

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
                name: "fk_curricula_departments_department_uid",
                table: "curricula",
                column: "department_uid",
                principalTable: "departments",
                principalColumn: "uid",
                onDelete: ReferentialAction.SetNull);

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
                name: "lesson_progresses");

            migrationBuilder.DropTable(
                name: "library_loans");

            migrationBuilder.DropTable(
                name: "notification_settings");

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
                name: "system_settings");

            migrationBuilder.DropTable(
                name: "library_resources");

            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "grades");

            migrationBuilder.DropTable(
                name: "assignments");

            migrationBuilder.DropTable(
                name: "exams");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "lessons");

            migrationBuilder.DropTable(
                name: "course_instances");

            migrationBuilder.DropTable(
                name: "academic_periods");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "curricula");

            migrationBuilder.DropTable(
                name: "persons");

            migrationBuilder.DropTable(
                name: "teachers");

            migrationBuilder.DropTable(
                name: "departments");
        }
    }
}
