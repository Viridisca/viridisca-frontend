using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ViridiscaUi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "user_uid",
                table: "teachers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_uid",
                table: "students",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "uid", "code", "created_at", "description", "head_of_department_uid", "is_active", "last_modified_at", "name" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "IT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кафедра информационных технологий и программирования", null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Информационные технологии" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "MATH", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кафедра высшей математики и математического анализа", null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Математический анализ" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "PHYS", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кафедра общей физики", null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Физика" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "uid", "created_at", "description", "last_modified_at", "name", "role_type" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Системный администратор", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SystemAdmin", 1 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Преподаватель", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teacher", 4 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Студент", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Student", 6 }
                });

            migrationBuilder.InsertData(
                table: "subjects",
                columns: new[] { "uid", "code", "created_at", "credits", "department_uid", "department_uid1", "description", "is_active", "last_modified_at", "lessons_per_week", "name", "type" },
                values: new object[,]
                {
                    { new Guid("51111111-1111-1111-1111-111111111111"), "PROG101", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), null, "Введение в программирование на языке C#", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Основы программирования", 1 },
                    { new Guid("52222222-2222-2222-2222-222222222222"), "MATH201", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), null, "Дифференциальное и интегральное исчисление", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Математический анализ", 1 },
                    { new Guid("53333333-3333-3333-3333-333333333333"), "PHYS101", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), null, "Основы механики и термодинамики", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Общая физика", 1 }
                });

            migrationBuilder.InsertData(
                table: "teachers",
                columns: new[] { "uid", "academic_degree", "academic_title", "bio", "created_at", "department_uid", "employee_code", "first_name", "hire_date", "hourly_rate", "last_modified_at", "last_name", "middle_name", "phone", "specialization", "status", "termination_date", "user_uid", "user_uid1" },
                values: new object[,]
                {
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Доктор физико-математических наук", "Профессор", "Профессор математики, автор более 50 научных работ", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "T002", "Мария", new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1400m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Сидорова", "Петровна", "+7 (902) 345-67-89", "Математика", 1, null, null, null },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Кандидат физико-математических наук", "Старший преподаватель", "Специалист по теоретической физике и механике", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "T003", "Александр", new DateTime(2016, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1600m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Козлов", "Николаевич", "+7 (903) 456-78-90", "Физика", 1, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "uid", "created_at", "date_of_birth", "email", "first_name", "is_active", "is_email_confirmed", "last_login_at", "last_modified_at", "last_name", "middle_name", "password_hash", "phone_number", "profile_image_url", "role_id", "username" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1989, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@viridisca.local", "Админ", true, true, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Системы", "", "$2a$11$zB8qMTPJpxxzG4YCbtHVqeicEwEiVl3R9sbxvMFyp/LZh6HOei7HO", "", "", new Guid("11111111-1111-1111-1111-111111111111"), "admin" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1984, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "teacher@viridisca.local", "Преподаватель", true, true, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Тестовый", "Иванович", "$2a$11$tbIMTpxXyQlMI5VBHg/C7upPPPRuP9jBbRXQdfDWqpsBVE8.y.y4q", "+7 (900) 123-45-67", "", new Guid("22222222-2222-2222-2222-222222222222"), "teacher" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2004, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "student@viridisca.local", "Студент", true, true, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Тестовый", "Петрович", "$2a$11$k5lDUlQiStw0lSFiIvteaeMqM7E1BWK8LuAAtLA6LAsNr6PxsRXkS", "+7 (900) 987-65-43", "", new Guid("33333333-3333-3333-3333-333333333333"), "student" }
                });

            migrationBuilder.InsertData(
                table: "courses",
                columns: new[] { "uid", "category", "code", "created_at", "credits", "description", "end_date", "last_modified_at", "learning_outcomes", "max_enrollments", "name", "prerequisites", "start_date", "status", "subject_uid", "teacher_uid", "teacher_uid1", "title" },
                values: new object[,]
                {
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"), "Математика", "MATH201", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Дифференциальное и интегральное исчисление", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Владение методами дифференциального и интегрального исчисления", 40, "Математический анализ", "Школьная математика", new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), null, "Математический анализ" },
                    { new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc1"), "Физика", "PHYS101", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Основы механики и термодинамики", new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Понимание основных законов механики и термодинамики", 35, "Общая физика", "Школьная физика и математика", new DateTime(2023, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), null, "Общая физика" }
                });

            migrationBuilder.InsertData(
                table: "groups",
                columns: new[] { "uid", "code", "created_at", "curator_uid", "department_uid", "description", "end_date", "last_activity_date", "last_modified_at", "max_students", "name", "start_date", "status", "teacher_uid", "year" },
                values: new object[,]
                {
                    { new Guid("22222220-2222-2222-2222-222222222222"), "MAT-201", new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Математика, 2 курс, группа 1", null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, "МАТ-201", new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 2 },
                    { new Guid("33333330-3333-3333-3333-333333333333"), "PHYS-401", new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Физика, 4 курс, группа 1", null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 20, "ФИЗ-401", new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 4 }
                });

            migrationBuilder.InsertData(
                table: "teachers",
                columns: new[] { "uid", "academic_degree", "academic_title", "bio", "created_at", "department_uid", "employee_code", "first_name", "hire_date", "hourly_rate", "last_modified_at", "last_name", "middle_name", "phone", "specialization", "status", "termination_date", "user_uid", "user_uid1" },
                values: new object[] { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Кандидат технических наук", "Доцент", "Опытный преподаватель программирования с 5-летним стажем", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "T001", "Иван", new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1500m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Петров", "Иванович", "+7 (901) 234-56-78", "Программирование", 1, null, new Guid("55555555-5555-5555-5555-555555555555"), null });

            migrationBuilder.InsertData(
                table: "user_roles",
                columns: new[] { "uid", "assigned_at", "created_at", "expires_at", "is_active", "last_modified_at", "role_uid", "user_uid", "user_uid1" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111"), new Guid("44444444-4444-4444-4444-444444444444"), null },
                    { new Guid("88888888-8888-8888-8888-888888888888"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), new Guid("55555555-5555-5555-5555-555555555555"), null },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-333333333333"), new Guid("66666666-6666-6666-6666-666666666666"), null }
                });

            migrationBuilder.InsertData(
                table: "assignments",
                columns: new[] { "uid", "course_id", "course_uid", "course_uid1", "created_at", "description", "difficulty", "due_date", "instructions", "last_modified_at", "lesson_uid", "lesson_uid1", "max_grade", "max_score", "module_id", "module_uid", "status", "title", "type" },
                values: new object[,]
                {
                    { new Guid("62222222-2222-2222-2222-222222222222"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Вычисление производных функций", 0, new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Решить задачи 1-10 из учебника", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 50.0, 50.0, null, null, 1, "Домашнее задание - Производные", 0 },
                    { new Guid("63333333-3333-3333-3333-333333333333"), new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc1"), new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc1"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Комплексное исследование механических систем", 2, new DateTime(2024, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc), "Создать проект по моделированию физической системы", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 200.0, 200.0, null, null, 1, "Проект - Механика", 3 }
                });

            migrationBuilder.InsertData(
                table: "courses",
                columns: new[] { "uid", "category", "code", "created_at", "credits", "description", "end_date", "last_modified_at", "learning_outcomes", "max_enrollments", "name", "prerequisites", "start_date", "status", "subject_uid", "teacher_uid", "teacher_uid1", "title" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), "Программирование", "PROG101", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Изучение основ программирования на языке C#", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Понимание основных концепций программирования, умение создавать простые программы", 50, "Основы программирования", "", new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), null, "Основы программирования" });

            migrationBuilder.InsertData(
                table: "groups",
                columns: new[] { "uid", "code", "created_at", "curator_uid", "department_uid", "description", "end_date", "last_activity_date", "last_modified_at", "max_students", "name", "start_date", "status", "teacher_uid", "year" },
                values: new object[] { new Guid("11111110-1111-1111-1111-111111111111"), "IT-301", new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Информационные технологии, 3 курс, группа 1", null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 25, "ИТ-301", new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, 3 });

            migrationBuilder.InsertData(
                table: "students",
                columns: new[] { "uid", "address", "birth_date", "created_at", "email", "emergency_contact_name", "emergency_contact_phone", "enrollment_date", "first_name", "graduation_date", "group_uid", "is_active", "last_modified_at", "last_name", "medical_information", "middle_name", "phone", "phone_number", "status", "student_code", "user_uid", "user_uid1" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333330"), "", new DateTime(2005, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "dmitry.volkov@student.viridisca.local", "", "", new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Дмитрий", null, new Guid("22222220-2222-2222-2222-222222222222"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Волков", "", "Сергеевич", "+7 (912) 345-67-89", "+7 (912) 345-67-89", 1, "ST201001", null, null },
                    { new Guid("44444444-4444-4444-4444-444444444440"), "", new DateTime(2005, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "anna.kuznetsova@student.viridisca.local", "", "", new DateTime(2022, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Анна", null, new Guid("22222220-2222-2222-2222-222222222222"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Кузнецова", "", "Владимировна", "+7 (913) 456-78-90", "+7 (913) 456-78-90", 1, "ST201002", null, null },
                    { new Guid("55555555-5555-5555-5555-555555555550"), "", new DateTime(2002, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mikhail.morozov@student.viridisca.local", "", "", new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Михаил", null, new Guid("33333330-3333-3333-3333-333333333333"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Морозов", "", "Игоревич", "+7 (914) 567-89-01", "+7 (914) 567-89-01", 1, "ST401001", null, null }
                });

            migrationBuilder.InsertData(
                table: "assignments",
                columns: new[] { "uid", "course_id", "course_uid", "course_uid1", "created_at", "description", "difficulty", "due_date", "instructions", "last_modified_at", "lesson_uid", "lesson_uid1", "max_grade", "max_score", "module_id", "module_uid", "status", "title", "type" },
                values: new object[] { new Guid("61111111-1111-1111-1111-111111111111"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Выполнение базовых задач по программированию на C#", 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Создать консольное приложение с базовыми операциями", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, 100.0, 100.0, null, null, 1, "Лабораторная работа №1 - Основы C#", 4 });

            migrationBuilder.InsertData(
                table: "enrollments",
                columns: new[] { "uid", "completed_at", "course_uid", "course_uid1", "created_at", "enrollment_date", "last_modified_at", "status", "student_uid", "student_uid1" },
                values: new object[,]
                {
                    { new Guid("73333333-3333-3333-3333-333333333333"), null, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, new Guid("33333333-3333-3333-3333-333333333330"), null },
                    { new Guid("74444444-4444-4444-4444-444444444444"), null, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 12, 17, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, new Guid("44444444-4444-4444-4444-444444444440"), null }
                });

            migrationBuilder.InsertData(
                table: "grades",
                columns: new[] { "uid", "assignment_uid", "assignment_uid1", "comment", "created_at", "description", "graded_at", "graded_by_uid", "is_published", "issued_at", "last_modified_at", "lesson_uid", "published_at", "student_uid", "student_uid1", "subject_uid", "subject_uid1", "teacher_uid", "teacher_uid1", "type", "value" },
                values: new object[] { new Guid("83333333-3333-3333-3333-333333333333"), new Guid("62222222-2222-2222-2222-222222222222"), null, "Все задачи решены правильно.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Домашнее задание по производным", new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-333333333330"), null, new Guid("52222222-2222-2222-2222-222222222222"), null, new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), null, 0, 45m });

            migrationBuilder.InsertData(
                table: "students",
                columns: new[] { "uid", "address", "birth_date", "created_at", "email", "emergency_contact_name", "emergency_contact_phone", "enrollment_date", "first_name", "graduation_date", "group_uid", "is_active", "last_modified_at", "last_name", "medical_information", "middle_name", "phone", "phone_number", "status", "student_code", "user_uid", "user_uid1" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111110"), "", new DateTime(2004, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "alexey.ivanov@student.viridisca.local", "", "", new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Алексей", null, new Guid("11111110-1111-1111-1111-111111111111"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Иванов", "", "Петрович", "+7 (910) 123-45-67", "+7 (910) 123-45-67", 1, "ST301001", new Guid("66666666-6666-6666-6666-666666666666"), null },
                    { new Guid("22222222-2222-2222-2222-222222222220"), "", new DateTime(2003, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "elena.smirnova@student.viridisca.local", "", "", new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Елена", null, new Guid("11111110-1111-1111-1111-111111111111"), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Смирнова", "", "Александровна", "+7 (911) 234-56-78", "+7 (911) 234-56-78", 1, "ST301002", null, null }
                });

            migrationBuilder.InsertData(
                table: "enrollments",
                columns: new[] { "uid", "completed_at", "course_uid", "course_uid1", "created_at", "enrollment_date", "last_modified_at", "status", "student_uid", "student_uid1" },
                values: new object[,]
                {
                    { new Guid("71111111-1111-1111-1111-111111111111"), null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 12, 2, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, new Guid("11111111-1111-1111-1111-111111111110"), null },
                    { new Guid("72222222-2222-2222-2222-222222222222"), null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2023, 12, 7, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, new Guid("22222222-2222-2222-2222-222222222220"), null }
                });

            migrationBuilder.InsertData(
                table: "grades",
                columns: new[] { "uid", "assignment_uid", "assignment_uid1", "comment", "created_at", "description", "graded_at", "graded_by_uid", "is_published", "issued_at", "last_modified_at", "lesson_uid", "published_at", "student_uid", "student_uid1", "subject_uid", "subject_uid1", "teacher_uid", "teacher_uid1", "type", "value" },
                values: new object[,]
                {
                    { new Guid("81111111-1111-1111-1111-111111111111"), new Guid("61111111-1111-1111-1111-111111111111"), null, "Хорошая работа! Есть небольшие замечания по стилю кода.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Лабораторная работа по программированию", new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2023, 12, 28, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111110"), null, new Guid("51111111-1111-1111-1111-111111111111"), null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), null, 0, 85m },
                    { new Guid("82222222-2222-2222-2222-222222222222"), new Guid("61111111-1111-1111-1111-111111111111"), null, "Отличная работа! Код чистый и хорошо структурированный.", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Лабораторная работа по программированию", new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222220"), null, new Guid("51111111-1111-1111-1111-111111111111"), null, new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), null, 0, 92m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "assignments",
                keyColumn: "uid",
                keyValue: new Guid("63333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "enrollments",
                keyColumn: "uid",
                keyValue: new Guid("71111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "enrollments",
                keyColumn: "uid",
                keyValue: new Guid("72222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "enrollments",
                keyColumn: "uid",
                keyValue: new Guid("73333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "enrollments",
                keyColumn: "uid",
                keyValue: new Guid("74444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "grades",
                keyColumn: "uid",
                keyValue: new Guid("81111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "grades",
                keyColumn: "uid",
                keyValue: new Guid("82222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "grades",
                keyColumn: "uid",
                keyValue: new Guid("83333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "uid",
                keyValue: new Guid("55555555-5555-5555-5555-555555555550"));

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "uid",
                keyValue: new Guid("53333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "user_roles",
                keyColumn: "uid",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "user_roles",
                keyColumn: "uid",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "user_roles",
                keyColumn: "uid",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "assignments",
                keyColumn: "uid",
                keyValue: new Guid("61111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "assignments",
                keyColumn: "uid",
                keyValue: new Guid("62222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "courses",
                keyColumn: "uid",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc1"));

            migrationBuilder.DeleteData(
                table: "groups",
                keyColumn: "uid",
                keyValue: new Guid("33333330-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "uid",
                keyValue: new Guid("11111111-1111-1111-1111-111111111110"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "uid",
                keyValue: new Guid("22222222-2222-2222-2222-222222222220"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "uid",
                keyValue: new Guid("33333333-3333-3333-3333-333333333330"));

            migrationBuilder.DeleteData(
                table: "students",
                keyColumn: "uid",
                keyValue: new Guid("44444444-4444-4444-4444-444444444440"));

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "uid",
                keyValue: new Guid("51111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "subjects",
                keyColumn: "uid",
                keyValue: new Guid("52222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "uid",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "courses",
                keyColumn: "uid",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"));

            migrationBuilder.DeleteData(
                table: "courses",
                keyColumn: "uid",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"));

            migrationBuilder.DeleteData(
                table: "groups",
                keyColumn: "uid",
                keyValue: new Guid("11111110-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "groups",
                keyColumn: "uid",
                keyValue: new Guid("22222220-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "uid",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "teachers",
                keyColumn: "uid",
                keyValue: new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "uid",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "departments",
                keyColumn: "uid",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "uid",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "teachers",
                keyColumn: "uid",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));

            migrationBuilder.DeleteData(
                table: "teachers",
                keyColumn: "uid",
                keyValue: new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"));

            migrationBuilder.DeleteData(
                table: "departments",
                keyColumn: "uid",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "departments",
                keyColumn: "uid",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "uid",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "uid",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.AlterColumn<Guid>(
                name: "user_uid",
                table: "teachers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "user_uid",
                table: "students",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
