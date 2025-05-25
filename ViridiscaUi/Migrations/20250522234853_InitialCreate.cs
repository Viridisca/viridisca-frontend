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
                name: "subjects",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    lessons_per_week = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subjects", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    profile_image_url = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.uid);
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
                name: "teachers",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_code = table.Column<string>(type: "text", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: false),
                    hire_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    termination_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    academic_degree = table.Column<string>(type: "text", nullable: false),
                    academic_title = table.Column<string>(type: "text", nullable: false),
                    specialization = table.Column<string>(type: "text", nullable: false),
                    hourly_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    bio = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teachers", x => x.uid);
                    table.ForeignKey(
                        name: "fk_teachers_users_user_uid",
                        column: x => x.user_uid,
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
                name: "courses",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    teacher_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_courses", x => x.uid);
                    table.ForeignKey(
                        name: "fk_courses_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_courses_teachers_teacher_uid1",
                        column: x => x.teacher_uid1,
                        principalTable: "teachers",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_students = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    curator_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.uid);
                    table.ForeignKey(
                        name: "fk_groups_teachers_curator_uid",
                        column: x => x.curator_uid,
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
                name: "modules",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    course_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
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
                name: "students",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    student_code = table.Column<string>(type: "text", nullable: false),
                    enrollment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    emergency_contact_name = table.Column<string>(type: "text", nullable: false),
                    emergency_contact_phone = table.Column<string>(type: "text", nullable: false),
                    medical_information = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
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
                name: "lessons",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_cancelled = table.Column<bool>(type: "boolean", nullable: false),
                    cancellation_reason = table.Column<string>(type: "text", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    subject_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    teacher_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    group_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    module_uid = table.Column<Guid>(type: "uuid", nullable: true),
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
                        name: "fk_lessons_groups_group_uid1",
                        column: x => x.group_uid1,
                        principalTable: "groups",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_lessons_modules_module_uid",
                        column: x => x.module_uid,
                        principalTable: "modules",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_lessons_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lessons_subjects_subject_uid1",
                        column: x => x.subject_uid1,
                        principalTable: "subjects",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_lessons_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_lessons_teachers_teacher_uid1",
                        column: x => x.teacher_uid1,
                        principalTable: "teachers",
                        principalColumn: "uid");
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
                name: "student_parents",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    relation_type = table.Column<int>(type: "integer", nullable: false),
                    is_emergency_contact = table.Column<bool>(type: "boolean", nullable: false),
                    has_access_to_grades = table.Column<bool>(type: "boolean", nullable: false),
                    has_access_to_attendance = table.Column<bool>(type: "boolean", nullable: false),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
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
                        name: "fk_student_parents_students_student_uid1",
                        column: x => x.student_uid1,
                        principalTable: "students",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_student_parents_users_parent_user_uid",
                        column: x => x.parent_user_uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
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
                    type = table.Column<int>(type: "integer", nullable: false),
                    course_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    course_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    lesson_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignments", x => x.uid);
                    table.ForeignKey(
                        name: "fk_assignments_courses_course_uid",
                        column: x => x.course_uid,
                        principalTable: "courses",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_assignments_courses_course_uid1",
                        column: x => x.course_uid1,
                        principalTable: "courses",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_assignments_lessons_lesson_uid",
                        column: x => x.lesson_uid,
                        principalTable: "lessons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_assignments_lessons_lesson_uid1",
                        column: x => x.lesson_uid1,
                        principalTable: "lessons",
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
                    value = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lesson_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grades", x => x.uid);
                    table.ForeignKey(
                        name: "fk_grades_lessons_lesson_uid",
                        column: x => x.lesson_uid,
                        principalTable: "lessons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_grades_lessons_lesson_uid1",
                        column: x => x.lesson_uid1,
                        principalTable: "lessons",
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
                        name: "fk_grades_subjects_subject_uid1",
                        column: x => x.subject_uid1,
                        principalTable: "subjects",
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
                name: "submissions",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    submission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
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

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "uid", "created_at", "description", "last_modified_at", "name", "role_type" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Системный администратор", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrator", 1 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Преподаватель", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teacher", 4 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Студент", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Student", 6 }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "uid", "created_at", "date_of_birth", "email", "first_name", "is_active", "is_email_confirmed", "last_login_at", "last_modified_at", "last_name", "middle_name", "password_hash", "phone_number", "profile_image_url", "username" },
                values: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@viridisca.local", "Admin", true, false, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Viridisca", "", "$2a$11$8T8P3PotQP8V8pzF8H3w9.VJgQnODklQ4jNxeJV8Y1ZXzlGc0zP4e", "", "", "admin" });

            migrationBuilder.InsertData(
                table: "user_roles",
                columns: new[] { "uid", "assigned_at", "created_at", "expires_at", "is_active", "last_modified_at", "role_uid", "user_uid", "user_uid1" },
                values: new object[] { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111"), new Guid("44444444-4444-4444-4444-444444444444"), null });

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
                name: "ix_courses_teacher_uid",
                table: "courses",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_courses_teacher_uid1",
                table: "courses",
                column: "teacher_uid1");

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
                name: "ix_grades_lesson_uid",
                table: "grades",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_lesson_uid1",
                table: "grades",
                column: "lesson_uid1");

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
                name: "ix_groups_curator_uid",
                table: "groups",
                column: "curator_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_group_uid",
                table: "lessons",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_group_uid1",
                table: "lessons",
                column: "group_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_module_uid",
                table: "lessons",
                column: "module_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_subject_uid",
                table: "lessons",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_subject_uid1",
                table: "lessons",
                column: "subject_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_teacher_uid",
                table: "lessons",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_teacher_uid1",
                table: "lessons",
                column: "teacher_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_modules_course_uid",
                table: "modules",
                column: "course_uid");

            migrationBuilder.CreateIndex(
                name: "ix_modules_course_uid1",
                table: "modules",
                column: "course_uid1");

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
                name: "ix_student_parents_parent_user_uid",
                table: "student_parents",
                column: "parent_user_uid");

            migrationBuilder.CreateIndex(
                name: "ix_student_parents_student_uid_parent_user_uid",
                table: "student_parents",
                columns: new[] { "student_uid", "parent_user_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_student_parents_student_uid1",
                table: "student_parents",
                column: "student_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_students_group_uid",
                table: "students",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_students_user_uid",
                table: "students",
                column: "user_uid",
                unique: true);

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
                name: "ix_teachers_user_uid",
                table: "teachers",
                column: "user_uid",
                unique: true);

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
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enrollments");

            migrationBuilder.DropTable(
                name: "grade_comments");

            migrationBuilder.DropTable(
                name: "grade_revisions");

            migrationBuilder.DropTable(
                name: "role_permissions");

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
                name: "roles");

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
                name: "users");
        }
    }
}
