using Microsoft.EntityFrameworkCore;
using System;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.System;
using System.Collections.Generic;
using System.Text.Json;

namespace ViridiscaUi.Infrastructure;

/// <summary>
/// Entity Framework Core DbContext для работы с базой данных
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // Auth модели
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<AuthTokenInfo> AuthTokens { get; set; } = null!;

    // Education модели
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<Teacher> Teachers { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Module> Modules { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    public DbSet<LessonProgress> LessonProgress { get; set; } = null!;
    public DbSet<Assignment> Assignments { get; set; } = null!;
    public DbSet<Submission> Submissions { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    
    // Системные модели LMS
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<Schedule> Schedules { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;
    public DbSet<NotificationSettings> NotificationSettings { get; set; } = null!;
    public DbSet<FileRecord> FileRecords { get; set; } = null!;
    
    // Дополнительные модели LMS
    public DbSet<Grade> Grades { get; set; } = null!;
    public DbSet<GradeComment> GradeComments { get; set; } = null!;
    public DbSet<GradeRevision> GradeRevisions { get; set; } = null!;
    public DbSet<StudentParent> StudentParents { get; set; } = null!;
    public DbSet<TeacherSubject> TeacherSubjects { get; set; } = null!;
    public DbSet<TeacherGroup> TeacherGroups { get; set; } = null!;
    public DbSet<LessonDetail> LessonDetails { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфигурируем все модели LMS системы

        // Настройка моделей Auth
        ConfigureAuthModels(modelBuilder);

        // Настройка моделей Education
        ConfigureEducationModels(modelBuilder);
        
        // Настройка дополнительных моделей LMS
        ConfigureLmsModels(modelBuilder);
        
        // Настройка системных моделей
        ConfigureSystemModels(modelBuilder);

        // Seed начальных данных
        SeedInitialData(modelBuilder);
    }

    private void ConfigureAuthModels(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Основная роль пользователя (прямая связь)
            entity.HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
                
            // Дополнительные роли через UserRole (для системы множественных ролей)
            entity.HasMany(u => u.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Связи с профилями
            entity.HasOne(u => u.StudentProfile)
                .WithOne()
                .HasForeignKey<Student>(s => s.UserUid)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
                
            entity.HasOne(u => u.TeacherProfile)
                .WithOne()
                .HasForeignKey<Teacher>(t => t.UserUid)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // UserRole
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Навигационные свойства
            entity.HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => new { e.UserUid, e.RoleUid }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // RolePermission
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.HasOne<Role>().WithMany().HasForeignKey(e => e.RoleUid);
            entity.HasOne<Permission>().WithMany().HasForeignKey(e => e.PermissionUid);
            entity.HasIndex(e => new { e.RoleUid, e.PermissionUid }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // AuthTokenInfo
        modelBuilder.Entity<AuthTokenInfo>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.RefreshToken).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.RefreshToken);
            entity.HasIndex(e => e.ExpiresAt);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private void ConfigureEducationModels(ModelBuilder modelBuilder)
    {
        // Student
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.StudentCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(200);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.MedicalInformation).HasMaxLength(1000);
            entity.Property(e => e.Address).HasMaxLength(500);
            
            // Даты и статус
            entity.Property(e => e.BirthDate).IsRequired();
            entity.Property(e => e.EnrollmentDate).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.GraduationDate).IsRequired(false);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            // Индексы
            entity.HasIndex(e => e.StudentCode).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.EnrollmentDate);
            
            // Связь с пользователем (One-to-One)
            entity.HasOne<User>()
                .WithOne(u => u.StudentProfile)
                .HasForeignKey<Student>(s => s.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Связь с группой
            entity.HasOne(s => s.Group)
                .WithMany(g => g.Students)
                .HasForeignKey(s => s.GroupUid)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Коллекция родителей настраивается через StudentParent
            entity.HasMany(s => s.Parents)
                .WithOne()
                .HasForeignKey(sp => sp.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Group
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Year).IsRequired();
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired(false);
            entity.Property(e => e.MaxStudents).IsRequired();
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            
            // Индексы
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Year);
            entity.HasIndex(e => e.StartDate);
            
            // Связь с куратором
            entity.HasOne(g => g.Curator)
                .WithMany()
                .HasForeignKey(g => g.CuratorUid)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            
            // Связь с департаментом
            entity.HasOne<Department>()
                .WithMany(d => d.Groups)
                .HasForeignKey(g => g.DepartmentUid)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Teacher
        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AcademicDegree).HasMaxLength(200);
            entity.Property(e => e.AcademicTitle).HasMaxLength(200);
            entity.Property(e => e.Specialization).HasMaxLength(300);
            entity.Property(e => e.Bio).HasMaxLength(2000);
            entity.Property(e => e.HourlyRate).HasPrecision(10, 2);
            
            // Даты и статус
            entity.Property(e => e.HireDate).IsRequired();
            entity.Property(e => e.TerminationDate).IsRequired(false);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            
            // Индексы
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.HireDate);
            
            // Связь с пользователем (One-to-One)
            entity.HasOne<User>()
                .WithOne(u => u.TeacherProfile)
                .HasForeignKey<Teacher>(t => t.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Связь с департаментом (если есть)
            entity.HasOne<Department>()
                .WithMany(d => d.Teachers)
                .HasForeignKey(t => t.DepartmentUid)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            
            // Связи через промежуточные таблицы
            entity.HasMany(t => t.Subjects)
                .WithOne()
                .HasForeignKey(ts => ts.TeacherUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany<TeacherGroup>()
                .WithOne()
                .HasForeignKey(tg => tg.TeacherUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Course
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            
            // Связь с преподавателем
            entity.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(c => c.TeacherUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Module
        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            // Связь с курсом
            entity.HasOne<Course>()
                .WithMany()
                .HasForeignKey(m => m.CourseUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Lesson
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            // Связь с предметом
            entity.HasOne<Subject>()
                .WithMany()
                .HasForeignKey(l => l.SubjectUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Связь с преподавателем
            entity.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(l => l.TeacherUid)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Связь с группой
            entity.HasOne<Group>()
                .WithMany()
                .HasForeignKey(l => l.GroupUid)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Связь с оценками
            entity.HasMany<Grade>()
                .WithOne()
                .HasForeignKey(g => g.LessonUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // LessonProgress
        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.StudentUid).IsRequired();
            entity.Property(e => e.LessonUid).IsRequired();
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
            entity.Property(e => e.CompletedAt).IsRequired(false);
            entity.Property(e => e.TimeSpent).IsRequired(false);
            
            // Связь со студентом
            entity.HasOne(lp => lp.Student)
                .WithMany()
                .HasForeignKey(lp => lp.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Связь с уроком
            entity.HasOne(lp => lp.Lesson)
                .WithMany(l => l.LessonProgress)
                .HasForeignKey(lp => lp.LessonUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Уникальный индекс: один студент - один урок - один прогресс
            entity.HasIndex(e => new { e.StudentUid, e.LessonUid }).IsUnique();
            
            // Индексы для быстрого поиска
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.CompletedAt);
        });

        // Assignment
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            // Связь с курсом
            entity.HasOne<Course>()
                .WithMany()
                .HasForeignKey(a => a.CourseUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Связь с уроком (опциональная)
            entity.HasOne<Lesson>()
                .WithMany()
                .HasForeignKey(a => a.LessonUid)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        // Submission
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Content).HasColumnType("text");
            
            // Связь со студентом
            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(s => s.StudentUid);
            
            // Связь с заданием
            entity.HasOne<Assignment>()
                .WithMany()
                .HasForeignKey(s => s.AssignmentUid);
        });

        // Enrollment
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.EnrollmentDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связь со студентом
            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(e => e.StudentUid);
            
            // Связь с курсом
            entity.HasOne<Course>()
                .WithMany()
                .HasForeignKey(e => e.CourseUid);
        });

        // Subject
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Credits).IsRequired();
            entity.Property(e => e.LessonsPerWeek).IsRequired();
            entity.Property(e => e.Type).HasConversion<int>().IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            // Индексы
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
            
            // Связь с департаментом
            entity.HasOne<Department>()
                .WithMany(d => d.Subjects)
                .HasForeignKey(s => s.DepartmentUid)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Связи через промежуточные таблицы
            entity.HasMany(s => s.TeacherSubjects)
                .WithOne()
                .HasForeignKey(ts => ts.SubjectUid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureLmsModels(ModelBuilder modelBuilder)
    {
        // Grade
        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Value).IsRequired().HasPrecision(5, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IssuedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(g => g.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne<Subject>()
                .WithMany()
                .HasForeignKey(g => g.SubjectUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(g => g.TeacherUid)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne<Lesson>()
                .WithMany()
                .HasForeignKey(g => g.LessonUid)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
                
            // Связи с комментариями и ревизиями
            entity.HasMany<GradeComment>()
                .WithOne()
                .HasForeignKey(gc => gc.GradeUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany<GradeRevision>()
                .WithOne()
                .HasForeignKey(gr => gr.GradeUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GradeComment
        modelBuilder.Entity<GradeComment>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
            
            // Связь с оценкой
            entity.HasOne<Grade>()
                .WithMany()
                .HasForeignKey(gc => gc.GradeUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GradeRevision
        modelBuilder.Entity<GradeRevision>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.PreviousValue).HasPrecision(5, 2);
            entity.Property(e => e.NewValue).HasPrecision(5, 2);
            entity.Property(e => e.RevisionReason).HasMaxLength(500);
            entity.Property(e => e.PreviousDescription).HasMaxLength(500);
            entity.Property(e => e.NewDescription).HasMaxLength(500);
            
            // Связь с оценкой
            entity.HasOne<Grade>()
                .WithMany()
                .HasForeignKey(gr => gr.GradeUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StudentParent
        modelBuilder.Entity<StudentParent>(entity =>
        {
            entity.HasKey(e => e.Uid);
            
            // Связи
            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(sp => sp.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(sp => sp.ParentUserUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Уникальный индекс
            entity.HasIndex(e => new { e.StudentUid, e.ParentUserUid }).IsUnique();
        });

        // TeacherSubject
        modelBuilder.Entity<TeacherSubject>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.AssignedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(ts => ts.TeacherUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne<Subject>()
                .WithMany()
                .HasForeignKey(ts => ts.SubjectUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Уникальный индекс
            entity.HasIndex(e => new { e.TeacherUid, e.SubjectUid }).IsUnique();
        });

        // TeacherGroup
        modelBuilder.Entity<TeacherGroup>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(tg => tg.TeacherUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne<Group>()
                .WithMany()
                .HasForeignKey(tg => tg.GroupUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne<Subject>()
                .WithMany()
                .HasForeignKey(tg => tg.SubjectUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Уникальный индекс для связки преподаватель-группа-предмет
            entity.HasIndex(e => new { e.TeacherUid, e.GroupUid, e.SubjectUid }).IsUnique();
        });

        // LessonDetail - детальная информация об уроках для отчетности
        modelBuilder.Entity<LessonDetail>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.TeacherFirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TeacherLastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TeacherMiddleName).HasMaxLength(100);
            entity.Property(e => e.SubjectName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.GroupName).IsRequired().HasMaxLength(100);
            
            // Связь с основным уроком
            entity.HasOne<Lesson>()
                .WithMany()
                .HasForeignKey(ld => ld.LessonUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Индекс для быстрого поиска по уроку
            entity.HasIndex(e => e.LessonUid);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => new { e.TeacherLastName, e.TeacherFirstName });
        });
    }

    private void ConfigureSystemModels(ModelBuilder modelBuilder)
    {
        // Department
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.Code).IsUnique();
            
            // Связь с заведующим кафедрой
            entity.HasOne(d => d.HeadOfDepartment)
                .WithMany()
                .HasForeignKey(d => d.HeadOfDepartmentUid)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
                
            // Связи с предметами и группами
            entity.HasMany(d => d.Subjects)
                .WithOne()
                .HasForeignKey(s => s.DepartmentUid)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasMany(d => d.Groups)
                .WithOne()
                .HasForeignKey(g => g.DepartmentUid)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Attendance
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CheckedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(a => a.Lesson)
                .WithMany()
                .HasForeignKey(a => a.LessonUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(a => a.CheckedByUid)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Уникальный индекс: один студент - один урок - одна запись посещаемости
            entity.HasIndex(e => new { e.StudentUid, e.LessonUid }).IsUnique();
        });

        // Schedule
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Classroom).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ValidFrom).HasDefaultValueSql("CURRENT_DATE");
            
            // Связи
            entity.HasOne(s => s.Group)
                .WithMany()
                .HasForeignKey(s => s.GroupUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(s => s.Subject)
                .WithMany()
                .HasForeignKey(s => s.SubjectUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(s => s.Teacher)
                .WithMany()
                .HasForeignKey(s => s.TeacherUid)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Индексы для быстрого поиска
            entity.HasIndex(e => new { e.GroupUid, e.DayOfWeek, e.StartTime });
            entity.HasIndex(e => new { e.TeacherUid, e.DayOfWeek, e.StartTime });
            entity.HasIndex(e => e.ValidFrom);
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.SentAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Сериализуем Metadata в JSON для хранения в БД
            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null))
                .HasColumnType("text");
            
            // Связь с получателем
            entity.HasOne(n => n.Recipient)
                .WithMany()
                .HasForeignKey(n => n.RecipientUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Индексы
            entity.HasIndex(e => new { e.RecipientUid, e.IsRead });
            entity.HasIndex(e => e.SentAt);
            entity.HasIndex(e => e.Type);
        });

        // NotificationTemplate
        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TitleTemplate).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MessageTemplate).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            // Сериализуем Parameters в JSON для хранения в БД
            entity.Property(e => e.Parameters)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("text");
            
            // Индексы
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
        });

        // NotificationSettings
        modelBuilder.Entity<NotificationSettings>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.UserUid).IsRequired();
            entity.Property(e => e.EmailNotifications).HasDefaultValue(true);
            entity.Property(e => e.PushNotifications).HasDefaultValue(true);
            entity.Property(e => e.SmsNotifications).HasDefaultValue(false);
            entity.Property(e => e.WeekendNotifications).HasDefaultValue(false);
            entity.Property(e => e.MinimumPriority).HasConversion<int>().HasDefaultValue(NotificationPriority.Low);
            
            // Сериализуем TypeSettings в JSON для хранения в БД
            entity.Property(e => e.TypeSettings)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<NotificationType, bool>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<NotificationType, bool>())
                .HasColumnType("text");
            
            // Сериализуем CategorySettings в JSON для хранения в БД
            entity.Property(e => e.CategorySettings)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, bool>())
                .HasColumnType("text");
            
            // Связь с пользователем
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(ns => ns.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Уникальный индекс: один пользователь - одни настройки
            entity.HasIndex(e => e.UserUid).IsUnique();
        });

        // FileRecord
        modelBuilder.Entity<FileRecord>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Индексы для быстрого поиска
            entity.HasIndex(e => e.UploadedByUid);
            entity.HasIndex(e => new { e.EntityType, e.EntityUid });
            entity.HasIndex(e => e.ContentType);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.IsPublic);
        });
    }

    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Фиксированные значения для seed данных
        var adminRoleId = new Guid("11111111-1111-1111-1111-111111111111");
        var teacherRoleId = new Guid("22222222-2222-2222-2222-222222222222");
        var studentRoleId = new Guid("33333333-3333-3333-3333-333333333333");
        var adminUserId = new Guid("44444444-4444-4444-4444-444444444444");
        var userRoleId = new Guid("55555555-5555-5555-5555-555555555555");
        var itDeptId = new Guid("66666666-6666-6666-6666-666666666666");
        var mathDeptId = new Guid("77777777-7777-7777-7777-777777777777");
        var baseDateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Uid = adminRoleId,
                Name = "Administrator",
                Description = "Системный администратор",
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime,
                RoleType = RoleType.SystemAdmin
            },
            new Role
            {
                Uid = teacherRoleId,
                Name = "Teacher",
                Description = "Преподаватель",
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime,
                RoleType = RoleType.Teacher
            },
            new Role
            {
                Uid = studentRoleId,
                Name = "Student",
                Description = "Студент",
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime,
                RoleType = RoleType.Student
            }
        );

        // Seed admin user
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Uid = adminUserId,
                Username = "admin",
                Email = "admin@viridisca.local",
                FirstName = "Admin",
                LastName = "Viridisca",
                MiddleName = "",
                PhoneNumber = "",
                ProfileImageUrl = "",
                RoleId = adminRoleId, // Основная роль - администратор
                IsActive = true,
                IsEmailConfirmed = true,
                DateOfBirth = baseDateTime,
                PasswordHash = "$2a$11$8EPP7eDbOSFPG6YcVEWzsu81jRo550.5.b9INI7muBAMpfwa3ftcS", // admin123 (правильный хеш)
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // Связываем админа с ролью администратора  
        modelBuilder.Entity<UserRole>().HasData(
            new 
            {
                Uid = userRoleId,
                UserUid = adminUserId,
                RoleUid = adminRoleId,
                IsActive = true,
                AssignedAt = baseDateTime,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // Seed departments
        modelBuilder.Entity<Department>().HasData(
            new Department
            {
                Uid = itDeptId,
                Name = "Информационные технологии",
                Code = "IT",
                Description = "Кафедра информационных технологий и программирования",
                IsActive = true,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Department
            {
                Uid = mathDeptId,
                Name = "Математический анализ",
                Code = "MATH",
                Description = "Кафедра высшей математики и математического анализа",
                IsActive = true,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );
    } 
}
