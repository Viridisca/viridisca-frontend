using Microsoft.EntityFrameworkCore;
using System;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.System;
using System.Collections.Generic;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.Domain.Models.System.Enums;
using System.Linq;

namespace ViridiscaUi.Infrastructure;

/// <summary>
/// Entity Framework Core DbContext для работы с базой данных
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // ===== НОВАЯ СИСТЕМА АУТЕНТИФИКАЦИИ =====
    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<PersonRole> PersonRoles { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    // ===== АКАДЕМИЧЕСКАЯ СТРУКТУРА =====
    public DbSet<AcademicPeriod> AcademicPeriods { get; set; } = null!;
    public DbSet<Curriculum> Curricula { get; set; } = null!;
    public DbSet<CurriculumSubject> CurriculumSubjects { get; set; } = null!;
    public DbSet<CourseInstance> CourseInstances { get; set; } = null!;
    public DbSet<ScheduleSlot> ScheduleSlots { get; set; } = null!;

    // ===== ОСНОВНЫЕ ОБРАЗОВАТЕЛЬНЫЕ СУЩНОСТИ =====
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Teacher> Teachers { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    public DbSet<Assignment> Assignments { get; set; } = null!;
    public DbSet<Submission> Submissions { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;
    public DbSet<Grade> Grades { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;

    // ===== СИСТЕМА ЭКЗАМЕНОВ =====
    public DbSet<Exam> Exams { get; set; } = null!;
    public DbSet<ExamResult> ExamResults { get; set; } = null!;

    // ===== БИБЛИОТЕЧНАЯ СИСТЕМА =====
    public DbSet<LibraryResource> LibraryResources { get; set; } = null!;
    public DbSet<LibraryLoan> LibraryLoans { get; set; } = null!;
    
    // ===== СИСТЕМНЫЕ МОДЕЛИ =====
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;
    public DbSet<NotificationSettings> NotificationSettings { get; set; } = null!;
    public DbSet<FileRecord> FileRecords { get; set; } = null!;

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

        // Конфигурируем все модели новой LMS системы
        ConfigureAuthModels(modelBuilder);
        ConfigureAcademicModels(modelBuilder);
        ConfigureEducationModels(modelBuilder);
        ConfigureExamModels(modelBuilder);
        ConfigureLibraryModels(modelBuilder);
        ConfigureSystemModels(modelBuilder);

        // Seed начальных данных
        SeedInitialData(modelBuilder);
    }

    private void ConfigureAuthModels(ModelBuilder modelBuilder)
    {
        // Person - базовая сущность для всех людей
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MiddleName).HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.Property(e => e.Address).HasMaxLength(500);
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.PhoneNumber);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи с ролями
            entity.HasMany(p => p.PersonRoles)
                .WithOne(pr => pr.Person)
                .HasForeignKey(pr => pr.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Account - аутентификация
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.IsLocked).HasDefaultValue(false);
            entity.Property(e => e.FailedLoginAttempts).HasDefaultValue(0);
            
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связь с Person
            entity.HasOne(a => a.Person)
                .WithOne()
                .HasForeignKey<Account>(a => a.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PersonRole - гибкая система ролей
        modelBuilder.Entity<PersonRole>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.AssignedBy).HasMaxLength(100);
            entity.Property(e => e.Context).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e => new { e.PersonUid, e.RoleUid, e.Context });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne(pr => pr.Person)
                .WithMany(p => p.PersonRoles)
                .HasForeignKey(pr => pr.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(pr => pr.Role)
                .WithMany()
                .HasForeignKey(pr => pr.RoleUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // RolePermission
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Uid);
            
            entity.HasOne<Role>()
                .WithMany()
                .HasForeignKey(e => e.RoleUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne<Permission>()
                .WithMany()
                .HasForeignKey(e => e.PermissionUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => new { e.RoleUid, e.PermissionUid }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private void ConfigureAcademicModels(ModelBuilder modelBuilder)
    {
        // AcademicPeriod
        modelBuilder.Entity<AcademicPeriod>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).HasConversion<int>().IsRequired();
            entity.Property(e => e.AcademicYear).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasIndex(e => new { e.AcademicYear, e.Type });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Curriculum
        modelBuilder.Entity<Curriculum>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.TotalCredits).IsRequired();
            entity.Property(e => e.DurationInSemesters).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // CurriculumSubject
        modelBuilder.Entity<CurriculumSubject>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Semester).IsRequired();
            entity.Property(e => e.Credits).IsRequired();
            entity.Property(e => e.IsRequired).HasDefaultValue(true);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasIndex(e => new { e.CurriculumUid, e.SubjectUid }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne(cs => cs.Curriculum)
                .WithMany(c => c.CurriculumSubjects)
                .HasForeignKey(cs => cs.CurriculumUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(cs => cs.Subject)
                .WithMany()
                .HasForeignKey(cs => cs.SubjectUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CourseInstance
        modelBuilder.Entity<CourseInstance>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.MaxStudents).HasDefaultValue(30);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasIndex(e => new { e.SubjectUid, e.GroupUid, e.AcademicPeriodUid });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne(ci => ci.Subject)
                .WithMany()
                .HasForeignKey(ci => ci.SubjectUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(ci => ci.Group)
                .WithMany()
                .HasForeignKey(ci => ci.GroupUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ci => ci.AcademicPeriod)
                .WithMany(ap => ap.CourseInstances)
                .HasForeignKey(ci => ci.AcademicPeriodUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ci => ci.Teacher)
                .WithMany()
                .HasForeignKey(ci => ci.TeacherUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ScheduleSlot
        modelBuilder.Entity<ScheduleSlot>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Classroom).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasIndex(e => new { e.CourseInstanceUid, e.DayOfWeek, e.StartTime });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связь с экземпляром курса
            entity.HasOne(ss => ss.CourseInstance)
                .WithMany(ci => ci.ScheduleSlots)
                .HasForeignKey(ss => ss.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureEducationModels(ModelBuilder modelBuilder)
    {
        // Student
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.StudentCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.GPA).HasPrecision(4, 2);
            
            entity.HasIndex(e => e.StudentCode).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne(s => s.Person)
                .WithOne()
                .HasForeignKey<Student>(s => s.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(s => s.Group)
                .WithMany()
                .HasForeignKey(s => s.GroupUid)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(s => s.Curriculum)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.CurriculumUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Teacher
        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Qualification).HasMaxLength(200);
            entity.Property(e => e.Specialization).HasMaxLength(300);
            entity.Property(e => e.OfficeLocation).HasMaxLength(100);
            entity.Property(e => e.Salary).HasPrecision(10, 2);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne(t => t.Person)
                .WithOne()
                .HasForeignKey<Teacher>(t => t.PersonUid)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(t => t.Department)
                .WithMany()
                .HasForeignKey(t => t.DepartmentUid)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Group
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Subject
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Type).HasConversion<int>().IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Type);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связь с департаментом
            entity.HasOne<Department>()
                .WithMany()
                .HasForeignKey(s => s.DepartmentUid)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Enrollment
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.FinalGrade).HasPrecision(5, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            entity.HasIndex(e => new { e.StudentUid, e.CourseInstanceUid }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.CourseInstance)
                .WithMany(ci => ci.Enrollments)
                .HasForeignKey(e => e.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Grade
        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Value).IsRequired().HasPrecision(5, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.Type).HasConversion<int>().IsRequired();
            entity.Property(e => e.IsPublished).HasDefaultValue(false);
            
            entity.HasIndex(e => e.StudentUid);
            entity.HasIndex(e => e.IsPublished);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne<Student>()
                .WithMany(s => s.Grades)
                .HasForeignKey(g => g.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GradeComment
        modelBuilder.Entity<GradeComment>(entity =>
        {
            entity.HasKey(gc => gc.Uid);
            entity.Property(gc => gc.GradeUid).IsRequired();
            entity.Property(gc => gc.AuthorUid).IsRequired();
            entity.Property(gc => gc.Type).IsRequired();
            entity.Property(gc => gc.Content).IsRequired().HasMaxLength(2000);
            entity.Property(gc => gc.Status).IsRequired();
            entity.Property(gc => gc.IsDeleted).IsRequired();
            entity.Property(gc => gc.DeletedAt);

            // Связь с Grade
            entity.HasOne<Grade>()
                .WithMany()
                .HasForeignKey(gc => gc.GradeUid)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с Person (автор)
            entity.HasOne<Person>()
                .WithMany()
                .HasForeignKey(gc => gc.AuthorUid)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Attendance
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Status).HasConversion<int>().IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            entity.HasIndex(e => new { e.StudentUid, e.LessonUid }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Assignment
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(a => a.Uid);
            entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Description).HasMaxLength(2000);
            entity.Property(a => a.Instructions).HasMaxLength(5000);
            entity.Property(a => a.MaxScore).IsRequired();
            entity.Property(a => a.Type).IsRequired();
            entity.Property(a => a.Difficulty).IsRequired();
            entity.Property(a => a.Status).IsRequired();
            entity.Property(a => a.CourseInstanceUid).IsRequired();
            entity.Property(a => a.LessonUid);
            entity.Property(a => a.DueDate);

            // Связь с CourseInstance
            entity.HasOne(a => a.CourseInstance)
                .WithMany()
                .HasForeignKey(a => a.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с Lesson (опционально)
            entity.HasOne(a => a.Lesson)
                .WithMany()
                .HasForeignKey(a => a.LessonUid)
                .OnDelete(DeleteBehavior.SetNull);

            // Связь с Submissions
            entity.HasMany(a => a.Submissions)
                .WithOne(s => s.Assignment)
                .HasForeignKey(s => s.AssignmentUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GradeRevision
        modelBuilder.Entity<GradeRevision>(entity =>
        {
            entity.HasKey(gr => gr.Uid);
            entity.Property(gr => gr.GradeUid).IsRequired();
            entity.Property(gr => gr.TeacherUid).IsRequired();
            entity.Property(gr => gr.PreviousValue).IsRequired().HasColumnType("decimal(5,2)");
            entity.Property(gr => gr.NewValue).IsRequired().HasColumnType("decimal(5,2)");
            entity.Property(gr => gr.PreviousDescription).HasMaxLength(1000);
            entity.Property(gr => gr.NewDescription).HasMaxLength(1000);
            entity.Property(gr => gr.RevisionReason).IsRequired().HasMaxLength(500);
            entity.Property(gr => gr.CreatedAt).IsRequired();

            // Связь с Grade
            entity.HasOne<Grade>()
                .WithMany()
                .HasForeignKey(gr => gr.GradeUid)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с Teacher
            entity.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(gr => gr.TeacherUid)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Lesson
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(l => l.Uid);
            entity.Property(l => l.Title).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Topic).HasMaxLength(200);
            entity.Property(l => l.Description).HasMaxLength(2000);
            entity.Property(l => l.Content).HasMaxLength(10000);
            entity.Property(l => l.CourseInstanceUid).IsRequired();
            entity.Property(l => l.OrderIndex).IsRequired();
            entity.Property(l => l.Duration);
            entity.Property(l => l.Type).IsRequired();
            entity.Property(l => l.IsPublished).IsRequired();
            entity.Property(l => l.SubjectUid);
            entity.Property(l => l.TeacherUid);
            entity.Property(l => l.GroupUid);

            // Связь с CourseInstance
            entity.HasOne(l => l.CourseInstance)
                .WithMany()
                .HasForeignKey(l => l.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с Subject (опционально)
            entity.HasOne<Subject>()
                .WithMany()
                .HasForeignKey(l => l.SubjectUid)
                .OnDelete(DeleteBehavior.SetNull);

            // Связь с Teacher (опционально)
            entity.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(l => l.TeacherUid)
                .OnDelete(DeleteBehavior.SetNull);

            // Связь с Group (опционально)
            entity.HasOne<Group>()
                .WithMany()
                .HasForeignKey(l => l.GroupUid)
                .OnDelete(DeleteBehavior.SetNull);

            // Связь с LessonProgress
            entity.HasMany(l => l.LessonProgress)
                .WithOne(lp => lp.Lesson)
                .HasForeignKey(lp => lp.LessonUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LessonProgress
        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasKey(lp => lp.Uid);
            entity.Property(lp => lp.StudentUid).IsRequired();
            entity.Property(lp => lp.LessonUid).IsRequired();
            entity.Property(lp => lp.IsCompleted).IsRequired();
            entity.Property(lp => lp.CompletedAt);
            entity.Property(lp => lp.TimeSpent);

            // Связь с Student
            entity.HasOne(lp => lp.Student)
                .WithMany()
                .HasForeignKey(lp => lp.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с Lesson
            entity.HasOne(lp => lp.Lesson)
                .WithMany(l => l.LessonProgress)
                .HasForeignKey(lp => lp.LessonUid)
                .OnDelete(DeleteBehavior.Cascade);

            // Уникальный индекс для комбинации студент-урок
            entity.HasIndex(lp => new { lp.StudentUid, lp.LessonUid })
                .IsUnique();
        });

        // Submission
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(s => s.Uid);
            entity.Property(s => s.StudentUid).IsRequired();
            entity.Property(s => s.AssignmentUid).IsRequired();
            entity.Property(s => s.SubmissionDate).IsRequired();
            entity.Property(s => s.Content).HasMaxLength(10000);
            entity.Property(s => s.Score).HasColumnType("decimal(5,2)");
            entity.Property(s => s.Feedback).HasMaxLength(2000);
            entity.Property(s => s.Status).IsRequired();
            entity.Property(s => s.GradedByUid);
            entity.Property(s => s.GradedDate);

            // Связь с Student
            entity.HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с Assignment
            entity.HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentUid)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с Teacher (кто поставил оценку)
            entity.HasOne(s => s.GradedBy)
                .WithMany()
                .HasForeignKey(s => s.GradedByUid)
                .OnDelete(DeleteBehavior.SetNull);

            // Уникальный индекс для комбинации студент-задание
            entity.HasIndex(s => new { s.StudentUid, s.AssignmentUid })
                .IsUnique();
        });
    }

    private void ConfigureExamModels(ModelBuilder modelBuilder)
    {
        // Exam
        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Type).HasConversion<int>().IsRequired();
            entity.Property(e => e.MaxScore).HasPrecision(6, 2);
            entity.Property(e => e.Instructions).HasMaxLength(2000);
            entity.Property(e => e.IsPublished).HasDefaultValue(false);
            
            entity.HasIndex(e => e.ExamDate);
            entity.HasIndex(e => e.IsPublished);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связь с экземпляром курса
            entity.HasOne(e => e.CourseInstance)
                .WithMany()
                .HasForeignKey(e => e.CourseInstanceUid)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ExamResult
        modelBuilder.Entity<ExamResult>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Score).HasPrecision(6, 2);
            entity.Property(e => e.Feedback).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.IsAbsent).HasDefaultValue(false);
            
            entity.HasIndex(e => new { e.ExamUid, e.StudentUid }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne(er => er.Exam)
                .WithMany(e => e.Results)
                .HasForeignKey(er => er.ExamUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(er => er.Student)
                .WithMany()
                .HasForeignKey(er => er.StudentUid)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureLibraryModels(ModelBuilder modelBuilder)
    {
        // LibraryResource
        modelBuilder.Entity<LibraryResource>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Author).HasMaxLength(200);
            entity.Property(e => e.ISBN).HasMaxLength(20);
            entity.Property(e => e.Publisher).HasMaxLength(200);
            entity.Property(e => e.Type).HasConversion<int>().IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.DigitalUrl).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.TotalCopies).HasDefaultValue(1);
            entity.Property(e => e.AvailableCopies).HasDefaultValue(1);
            entity.Property(e => e.IsDigital).HasDefaultValue(false);
            
            entity.HasIndex(e => e.ISBN);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Title);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // LibraryLoan
        modelBuilder.Entity<LibraryLoan>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.FineAmount).HasPrecision(8, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.IsReturned).HasDefaultValue(false);
            
            entity.HasIndex(e => e.BorrowerUid);
            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.IsReturned);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Связи
            entity.HasOne(ll => ll.Resource)
                .WithMany(lr => lr.Loans)
                .HasForeignKey(ll => ll.ResourceUid)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(ll => ll.Borrower)
                .WithMany()
                .HasForeignKey(ll => ll.BorrowerUid)
                .OnDelete(DeleteBehavior.Cascade);
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
            entity.Property(e => e.IsActive).HasDefaultValue(true);
                
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Type).HasConversion<int>().IsRequired();
            entity.Property(e => e.Priority).HasConversion<int>().IsRequired();
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.IsImportant).HasDefaultValue(false);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.ActionUrl).HasMaxLength(500);
            
            // Сериализуем Metadata в JSON
            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null))
                .HasColumnType("text");
            
            entity.HasIndex(e => new { e.RecipientUid, e.IsRead });
            entity.HasIndex(e => e.SentAt);
            entity.HasIndex(e => e.Type);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // NotificationTemplate
        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.Uid);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TitleTemplate).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MessageTemplate).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Type).HasConversion<int>().IsRequired();
            entity.Property(e => e.Priority).HasConversion<int>().IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            // Сериализуем Parameters в JSON
            entity.Property(e => e.Parameters)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("text");
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // NotificationSettings
        modelBuilder.Entity<NotificationSettings>(entity =>
        {
            entity.HasKey(ns => ns.Uid);
            entity.Property(ns => ns.UserUid).IsRequired();
            entity.Property(ns => ns.EmailNotifications).IsRequired();
            entity.Property(ns => ns.PushNotifications).IsRequired();
            entity.Property(ns => ns.SmsNotifications).IsRequired();
            entity.Property(ns => ns.QuietHoursStart).IsRequired();
            entity.Property(ns => ns.QuietHoursEnd).IsRequired();
            entity.Property(ns => ns.WeekendNotifications).IsRequired();
            entity.Property(ns => ns.MinimumPriority).IsRequired();

            // Сериализуем TypeSettings в JSON
            entity.Property(e => e.TypeSettings)
                .HasConversion(
                    v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => v == null ? new Dictionary<NotificationType, bool>() : System.Text.Json.JsonSerializer.Deserialize<Dictionary<NotificationType, bool>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<NotificationType, bool>())
                .HasColumnType("text");

            // Сериализуем CategorySettings в JSON
            entity.Property(e => e.CategorySettings)
                .HasConversion(
                    v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => v == null ? new Dictionary<string, bool>() : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, bool>())
                .HasColumnType("text");

            // Связь с Person
            entity.HasOne<Person>()
                .WithMany()
                .HasForeignKey(ns => ns.UserUid)
                .OnDelete(DeleteBehavior.Cascade);
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
            entity.Property(e => e.IsPublic).HasDefaultValue(false);
            
            entity.HasIndex(e => e.UploadedByUid);
            entity.HasIndex(e => new { e.EntityType, e.EntityUid });
            entity.HasIndex(e => e.ContentType);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Фиксированные значения для seed данных
        var adminRoleId = new Guid("11111111-1111-1111-1111-111111111111");
        var teacherRoleId = new Guid("22222222-2222-2222-2222-222222222222");
        var studentRoleId = new Guid("33333333-3333-3333-3333-333333333333");
        var adminPersonId = new Guid("44444444-4444-4444-4444-444444444444");
        var teacherPersonId = new Guid("55555555-5555-5555-5555-555555555555");
        var studentPersonId = new Guid("66666666-6666-6666-6666-666666666666");
        var adminAccountId = new Guid("77777777-7777-7777-7777-777777777777");
        var teacherAccountId = new Guid("88888888-8888-8888-8888-888888888888");
        var studentAccountId = new Guid("99999999-9999-9999-9999-999999999999");
        var itDeptId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var baseDateTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 1. Роли
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Uid = adminRoleId,
                Name = "SystemAdmin",
                Description = "Системный администратор",
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Role
            {
                Uid = teacherRoleId,
                Name = "Teacher",
                Description = "Преподаватель",
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Role
            {
                Uid = studentRoleId,
                Name = "Student",
                Description = "Студент",
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 2. Люди
        modelBuilder.Entity<Person>().HasData(
            new Person
            {
                Uid = adminPersonId,
                FirstName = "Админ",
                LastName = "Системы",
                MiddleName = "Владимирович",
                Email = "admin@viridisca.local",
                PhoneNumber = "",
                DateOfBirth = baseDateTime.AddYears(-35),
                IsActive = true,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Person
            {
                Uid = teacherPersonId,
                FirstName = "Преподаватель",
                LastName = "Тестовый",
                MiddleName = "Иванович",
                Email = "teacher@viridisca.local",
                PhoneNumber = "+7 (900) 123-45-67",
                DateOfBirth = baseDateTime.AddYears(-40),
                IsActive = true,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Person
            {
                Uid = studentPersonId,
                FirstName = "Студент",
                LastName = "Тестовый",
                MiddleName = "Петрович",
                Email = "student@viridisca.local",
                PhoneNumber = "+7 (900) 987-65-43",
                DateOfBirth = baseDateTime.AddYears(-20),
                IsActive = true,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 3. Аккаунты
        modelBuilder.Entity<Account>().HasData(
            new Account
            {
                Uid = adminAccountId,
                PersonUid = adminPersonId,
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                IsEmailConfirmed = true,
                IsLocked = false,
                FailedLoginAttempts = 0,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Account
            {
                Uid = teacherAccountId,
                PersonUid = teacherPersonId,
                Username = "teacher",
                PasswordHash = HashPassword("teacher123"),
                IsEmailConfirmed = true,
                IsLocked = false,
                FailedLoginAttempts = 0,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            },
            new Account
            {
                Uid = studentAccountId,
                PersonUid = studentPersonId,
                Username = "student",
                PasswordHash = HashPassword("student123"),
                IsEmailConfirmed = true,
                IsLocked = false,
                FailedLoginAttempts = 0,
                CreatedAt = baseDateTime,
                LastModifiedAt = baseDateTime
            }
        );

        // 4. Департамент
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
            }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        // Отключаем предупреждения о pending model changes для упрощения разработки
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }
}