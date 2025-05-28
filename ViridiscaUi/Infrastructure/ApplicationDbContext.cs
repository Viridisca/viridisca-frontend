using Microsoft.EntityFrameworkCore;
using System;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.System;
using System.Collections.Generic;
using BCrypt.Net;

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

    /// <summary>
    /// Метод для создания правильного BCrypt хеша пароля
    /// </summary>
    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

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
            entity.Property(e => e.MinimumPriority).HasConversion<int>().HasDefaultValue(ViridiscaUi.Domain.Models.System.NotificationPriority.Low);
            
            // Сериализуем TypeSettings в JSON для хранения в БД
            entity.Property(e => e.TypeSettings)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<ViridiscaUi.Domain.Models.System.NotificationType, bool>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<ViridiscaUi.Domain.Models.System.NotificationType, bool>())
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
        var teacherUserId = new Guid("55555555-5555-5555-5555-555555555555");
        var studentUserId = new Guid("66666666-6666-6666-6666-666666666666");
        var adminUserRoleId = new Guid("77777777-7777-7777-7777-777777777777");
        var teacherUserRoleId = new Guid("88888888-8888-8888-8888-888888888888");
        var studentUserRoleId = new Guid("99999999-9999-9999-9999-999999999999");
        var itDeptId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var mathDeptId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var physDeptId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var baseDateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Предварительно объявляем все ID, которые будут использоваться в разных секциях
        var subject1Id = new Guid("51111111-1111-1111-1111-111111111111");
        var subject2Id = new Guid("52222222-2222-2222-2222-222222222222");
        var subject3Id = new Guid("53333333-3333-3333-3333-333333333333");
        var assignment1Id = new Guid("61111111-1111-1111-1111-111111111111");
        var assignment2Id = new Guid("62222222-2222-2222-2222-222222222222");
        var assignment3Id = new Guid("63333333-3333-3333-3333-333333333333");
        var teacher1Id = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd");
        var teacher2Id = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        var teacher3Id = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
        var student1Id = new Guid("11111111-1111-1111-1111-111111111110");
        var student2Id = new Guid("22222222-2222-2222-2222-222222222220");
        var student3Id = new Guid("33333333-3333-3333-3333-333333333330");
        var student4Id = new Guid("44444444-4444-4444-4444-444444444440");
        var student5Id = new Guid("55555555-5555-5555-5555-555555555550");
        var group1Id = new Guid("11111110-1111-1111-1111-111111111111");
        var group2Id = new Guid("22222220-2222-2222-2222-222222222222");
        var group3Id = new Guid("33333330-3333-3333-3333-333333333333");
        var course1Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var course2Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");
        var course3Id = new Guid("cccccccc-cccc-cccc-cccc-ccccccccccc1");
        var enrollment1Id = new Guid("71111111-1111-1111-1111-111111111111");
        var enrollment2Id = new Guid("72222222-2222-2222-2222-222222222222");
        var enrollment3Id = new Guid("73333333-3333-3333-3333-333333333333");
        var enrollment4Id = new Guid("74444444-4444-4444-4444-444444444444");
        var grade1Id = new Guid("81111111-1111-1111-1111-111111111111");
        var grade2Id = new Guid("82222222-2222-2222-2222-222222222222");
        var grade3Id = new Guid("83333333-3333-3333-3333-333333333333");

        // 1. Роли
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Uid = adminRoleId,
                Name = "SystemAdmin",
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

        // 2. Пользователи
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Uid = adminUserId,
                Username = "admin",
                Email = "admin@viridisca.local",
                FirstName = "Админ",
                LastName = "Системы",
                MiddleName = "",
                PhoneNumber = "",
                ProfileImageUrl = "",
                RoleId = adminRoleId,
                IsActive = true,
                IsEmailConfirmed = true,
                DateOfBirth = baseDateTime.AddYears(-35),
                PasswordHash = HashPassword("admin123"),
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new User
            {
                Uid = teacherUserId,
                Username = "teacher",
                Email = "teacher@viridisca.local",
                FirstName = "Преподаватель",
                LastName = "Тестовый",
                MiddleName = "Иванович",
                PhoneNumber = "+7 (900) 123-45-67",
                ProfileImageUrl = "",
                RoleId = teacherRoleId,
                IsActive = true,
                IsEmailConfirmed = true,
                DateOfBirth = baseDateTime.AddYears(-40),
                PasswordHash = HashPassword("teacher123"),
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new User
            {
                Uid = studentUserId,
                Username = "student",
                Email = "student@viridisca.local",
                FirstName = "Студент",
                LastName = "Тестовый",
                MiddleName = "Петрович",
                PhoneNumber = "+7 (900) 987-65-43",
                ProfileImageUrl = "",
                RoleId = studentRoleId,
                IsActive = true,
                IsEmailConfirmed = true,
                DateOfBirth = baseDateTime.AddYears(-20),
                PasswordHash = HashPassword("student123"),
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 3. Связи пользователь-роль
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole
            {
                Uid = adminUserRoleId,
                UserUid = adminUserId,
                RoleUid = adminRoleId,
                IsActive = true,
                AssignedAt = baseDateTime,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new UserRole
            {
                Uid = teacherUserRoleId,
                UserUid = teacherUserId,
                RoleUid = teacherRoleId,
                IsActive = true,
                AssignedAt = baseDateTime,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new UserRole
            {
                Uid = studentUserRoleId,
                UserUid = studentUserId,
                RoleUid = studentRoleId,
                IsActive = true,
                AssignedAt = baseDateTime,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 4. Департаменты
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
            },
            new Department
            {
                Uid = physDeptId,
                Name = "Физика",
                Code = "PHYS",
                Description = "Кафедра общей физики",
                IsActive = true,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 5. Преподаватели
        modelBuilder.Entity<Teacher>().HasData(
            new Teacher
            {
                Uid = teacher1Id,
                FirstName = "Иван",
                LastName = "Петров",
                MiddleName = "Иванович",
                EmployeeCode = "T001",
                Phone = "+7 (901) 234-56-78",
                HireDate = baseDateTime.AddYears(-5),
                UserUid = teacherUserId,
                Status = TeacherStatus.Active,
                Specialization = "Программирование",
                HourlyRate = 1500m,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Teacher
            {
                Uid = teacher2Id,
                FirstName = "Мария",
                LastName = "Сидорова",
                MiddleName = "Петровна",
                EmployeeCode = "T002",
                Phone = "+7 (902) 345-67-89",
                HireDate = baseDateTime.AddYears(-3),
                UserUid = Guid.Empty,
                Status = TeacherStatus.Active,
                Specialization = "Математика",
                HourlyRate = 1400m,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Teacher
            {
                Uid = teacher3Id,
                FirstName = "Александр",
                LastName = "Козлов",
                MiddleName = "Николаевич",
                EmployeeCode = "T003",
                Phone = "+7 (903) 456-78-90",
                HireDate = baseDateTime.AddYears(-8),
                UserUid = Guid.Empty,
                Status = TeacherStatus.Active,
                Specialization = "Физика",
                HourlyRate = 1600m,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 6. Группы
        modelBuilder.Entity<Group>().HasData(
            new Group
            {
                Uid = group1Id,
                Name = "ИТ-301",
                Description = "Информационные технологии, 3 курс, группа 1",
                Year = 3,
                CreatedAt = baseDateTime.AddYears(-3),
                LastModifiedAt = baseDateTime
            },
            new Group
            {
                Uid = group2Id,
                Name = "МАТ-201",
                Description = "Математика, 2 курс, группа 1",
                Year = 2,
                CreatedAt = baseDateTime.AddYears(-2),
                LastModifiedAt = baseDateTime
            },
            new Group
            {
                Uid = group3Id,
                Name = "ФИЗ-401",
                Description = "Физика, 4 курс, группа 1",
                Year = 4,
                CreatedAt = baseDateTime.AddYears(-4),
                LastModifiedAt = baseDateTime
            }
        );

        // 7. Студенты
        modelBuilder.Entity<Student>().HasData(
            new Student
            {
                Uid = student1Id,
                FirstName = "Алексей",
                LastName = "Иванов",
                MiddleName = "Петрович",
                Email = "alexey.ivanov@student.viridisca.local",
                PhoneNumber = "+7 (910) 123-45-67",
                StudentCode = "ST301001",
                EnrollmentDate = baseDateTime.AddYears(-3),
                BirthDate = baseDateTime.AddYears(-20),
                GroupUid = group1Id,
                Status = StudentStatus.Active,
                IsActive = true,
                UserUid = studentUserId,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Student
            {
                Uid = student2Id,
                FirstName = "Елена",
                LastName = "Смирнова",
                MiddleName = "Александровна",
                Email = "elena.smirnova@student.viridisca.local",
                PhoneNumber = "+7 (911) 234-56-78",
                StudentCode = "ST301002",
                EnrollmentDate = baseDateTime.AddYears(-3),
                BirthDate = baseDateTime.AddYears(-21),
                GroupUid = group1Id,
                Status = StudentStatus.Active,
                IsActive = true,
                UserUid = Guid.Empty,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Student
            {
                Uid = student3Id,
                FirstName = "Дмитрий",
                LastName = "Волков",
                MiddleName = "Сергеевич",
                Email = "dmitry.volkov@student.viridisca.local",
                PhoneNumber = "+7 (912) 345-67-89",
                StudentCode = "ST201001",
                EnrollmentDate = baseDateTime.AddYears(-2),
                BirthDate = baseDateTime.AddYears(-19),
                GroupUid = group2Id,
                Status = StudentStatus.Active,
                IsActive = true,
                UserUid = Guid.Empty,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Student
            {
                Uid = student4Id,
                FirstName = "Анна",
                LastName = "Кузнецова",
                MiddleName = "Владимировна",
                Email = "anna.kuznetsova@student.viridisca.local",
                PhoneNumber = "+7 (913) 456-78-90",
                StudentCode = "ST201002",
                EnrollmentDate = baseDateTime.AddYears(-2),
                BirthDate = baseDateTime.AddYears(-19),
                GroupUid = group2Id,
                Status = StudentStatus.Active,
                IsActive = true,
                UserUid = Guid.Empty,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Student
            {
                Uid = student5Id,
                FirstName = "Михаил",
                LastName = "Морозов",
                MiddleName = "Игоревич",
                Email = "mikhail.morozov@student.viridisca.local",
                PhoneNumber = "+7 (914) 567-89-01",
                StudentCode = "ST401001",
                EnrollmentDate = baseDateTime.AddYears(-4),
                BirthDate = baseDateTime.AddYears(-22),
                GroupUid = group3Id,
                Status = StudentStatus.Active,
                IsActive = true,
                UserUid = Guid.Empty,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 8. Курсы
        modelBuilder.Entity<Course>().HasData(
            new Course
            {
                Uid = course1Id,
                Name = "Основы программирования",
                Code = "PROG101",
                Description = "Изучение основ программирования на языке C#",
                Category = "Программирование",
                TeacherUid = teacher1Id,
                StartDate = baseDateTime.AddMonths(-2),
                EndDate = baseDateTime.AddMonths(4),
                Credits = 4,
                Status = CourseStatus.Active,
                Prerequisites = "",
                LearningOutcomes = "Понимание основных концепций программирования, умение создавать простые программы",
                MaxEnrollments = 50,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Course
            {
                Uid = course2Id,
                Name = "Математический анализ",
                Code = "MATH201",
                Description = "Дифференциальное и интегральное исчисление",
                Category = "Математика",
                TeacherUid = teacher2Id,
                StartDate = baseDateTime.AddMonths(-1),
                EndDate = baseDateTime.AddMonths(5),
                Credits = 5,
                Status = CourseStatus.Active,
                Prerequisites = "Школьная математика",
                LearningOutcomes = "Владение методами дифференциального и интегрального исчисления",
                MaxEnrollments = 40,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Course
            {
                Uid = course3Id,
                Name = "Общая физика",
                Code = "PHYS101",
                Description = "Основы механики и термодинамики",
                Category = "Физика",
                TeacherUid = teacher3Id,
                StartDate = baseDateTime.AddMonths(-3),
                EndDate = baseDateTime.AddMonths(3),
                Credits = 4,
                Status = CourseStatus.Active,
                Prerequisites = "Школьная физика и математика",
                LearningOutcomes = "Понимание основных законов механики и термодинамики",
                MaxEnrollments = 35,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 9. Записи на курсы
        modelBuilder.Entity<Enrollment>().HasData(
            new Enrollment
            {
                Uid = enrollment1Id,
                StudentUid = student1Id,
                CourseUid = course1Id,
                EnrollmentDate = baseDateTime.AddDays(-30),
                Status = EnrollmentStatus.Active,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Enrollment
            {
                Uid = enrollment2Id,
                StudentUid = student2Id,
                CourseUid = course1Id,
                EnrollmentDate = baseDateTime.AddDays(-25),
                Status = EnrollmentStatus.Active,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Enrollment
            {
                Uid = enrollment3Id,
                StudentUid = student3Id,
                CourseUid = course2Id,
                EnrollmentDate = baseDateTime.AddDays(-20),
                Status = EnrollmentStatus.Active,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Enrollment
            {
                Uid = enrollment4Id,
                StudentUid = student4Id,
                CourseUid = course2Id,
                EnrollmentDate = baseDateTime.AddDays(-15),
                Status = EnrollmentStatus.Active,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 10. Задания
        modelBuilder.Entity<Assignment>().HasData(
            new Assignment
            {
                Uid = assignment1Id,
                Title = "Лабораторная работа №1 - Основы C#",
                Description = "Выполнение базовых задач по программированию на C#",
                Instructions = "Создать консольное приложение с базовыми операциями",
                CourseUid = course1Id,
                DueDate = baseDateTime.AddDays(14),
                MaxScore = 100,
                Type = AssignmentType.LabWork,
                Difficulty = AssignmentDifficulty.Medium,
                Status = AssignmentStatus.Published,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Assignment
            {
                Uid = assignment2Id,
                Title = "Домашнее задание - Производные",
                Description = "Вычисление производных функций",
                Instructions = "Решить задачи 1-10 из учебника",
                CourseUid = course2Id,
                DueDate = baseDateTime.AddDays(7),
                MaxScore = 50,
                Type = AssignmentType.Homework,
                Difficulty = AssignmentDifficulty.Easy,
                Status = AssignmentStatus.Published,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Assignment
            {
                Uid = assignment3Id,
                Title = "Проект - Механика",
                Description = "Комплексное исследование механических систем",
                Instructions = "Создать проект по моделированию физической системы",
                CourseUid = course3Id,
                DueDate = baseDateTime.AddDays(30),
                MaxScore = 200,
                Type = AssignmentType.Project,
                Difficulty = AssignmentDifficulty.Hard,
                Status = AssignmentStatus.Published,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 11. Оценки
        modelBuilder.Entity<Grade>().HasData(
            new Grade
            {
                Uid = grade1Id,
                StudentUid = student1Id,
                SubjectUid = subject1Id, // Программирование
                TeacherUid = teacher1Id,
                AssignmentUid = assignment1Id,
                Value = 85m,
                Type = GradeType.Homework,
                Description = "Лабораторная работа по программированию",
                Comment = "Хорошая работа! Есть небольшие замечания по стилю кода.",
                IssuedAt = baseDateTime.AddDays(-5),
                GradedAt = baseDateTime.AddDays(-5),
                IsPublished = true,
                PublishedAt = baseDateTime.AddDays(-4),
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Grade
            {
                Uid = grade2Id,
                StudentUid = student2Id,
                SubjectUid = subject1Id, // Программирование
                TeacherUid = teacher1Id,
                AssignmentUid = assignment1Id,
                Value = 92m,
                Type = GradeType.Homework,
                Description = "Лабораторная работа по программированию",
                Comment = "Отличная работа! Код чистый и хорошо структурированный.",
                IssuedAt = baseDateTime.AddDays(-3),
                GradedAt = baseDateTime.AddDays(-3),
                IsPublished = true,
                PublishedAt = baseDateTime.AddDays(-2),
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Grade
            {
                Uid = grade3Id,
                StudentUid = student3Id,
                SubjectUid = subject2Id, // Математика
                TeacherUid = teacher2Id,
                AssignmentUid = assignment2Id,
                Value = 45m,
                Type = GradeType.Homework,
                Description = "Домашнее задание по производным",
                Comment = "Все задачи решены правильно.",
                IssuedAt = baseDateTime.AddDays(-1),
                GradedAt = baseDateTime.AddDays(-1),
                IsPublished = true,
                PublishedAt = baseDateTime,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 12. Предметы
        modelBuilder.Entity<Subject>().HasData(
            new Subject
            {
                Uid = subject1Id,
                Code = "PROG101",
                Name = "Основы программирования",
                Description = "Введение в программирование на языке C#",
                Credits = 4,
                LessonsPerWeek = 2,
                Type = SubjectType.Required,
                DepartmentUid = itDeptId,
                IsActive = true,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Subject
            {
                Uid = subject2Id,
                Code = "MATH201",
                Name = "Математический анализ",
                Description = "Дифференциальное и интегральное исчисление",
                Credits = 5,
                LessonsPerWeek = 3,
                Type = SubjectType.Required,
                DepartmentUid = mathDeptId,
                IsActive = true,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Subject
            {
                Uid = subject3Id,
                Code = "PHYS101",
                Name = "Общая физика",
                Description = "Основы механики и термодинамики",
                Credits = 4,
                LessonsPerWeek = 2,
                Type = SubjectType.Required,
                DepartmentUid = physDeptId,
                IsActive = true,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );
    }
}