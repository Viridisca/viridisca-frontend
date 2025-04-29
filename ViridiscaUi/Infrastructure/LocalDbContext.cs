using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using DynamicData;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.Education;
using ReactiveUI;
using BCrypt.Net;

namespace ViridiscaUi.Domain.Models.Education
{
    /// <summary>
    /// Модель курса
    /// </summary>
    public class Course : Base.ViewModelBase
    {
        private Guid _uid;
        private string _code = string.Empty;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private int _credits;
        private Guid _teacherUid;
        private Teacher? _teacher;
        private CourseStatus _status = CourseStatus.Draft;

        /// <summary>
        /// Уникальный идентификатор курса
        /// </summary>
        public new Guid Uid
        {
            get => _uid;
            set => this.RaiseAndSetIfChanged(ref _uid, value);
        }

        /// <summary>
        /// Код курса
        /// </summary>
        public string Code
        {
            get => _code;
            set => this.RaiseAndSetIfChanged(ref _code, value);
        }

        /// <summary>
        /// Название курса
        /// </summary>
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        /// <summary>
        /// Описание курса
        /// </summary>
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        /// <summary>
        /// Дата начала курса
        /// </summary>
        public DateTime? StartDate
        {
            get => _startDate;
            set => this.RaiseAndSetIfChanged(ref _startDate, value);
        }

        /// <summary>
        /// Дата окончания курса
        /// </summary>
        public DateTime? EndDate
        {
            get => _endDate;
            set => this.RaiseAndSetIfChanged(ref _endDate, value);
        }

        /// <summary>
        /// Количество кредитов за курс
        /// </summary>
        public int Credits
        {
            get => _credits;
            set => this.RaiseAndSetIfChanged(ref _credits, value);
        }

        /// <summary>
        /// Идентификатор преподавателя
        /// </summary>
        public Guid TeacherUid
        {
            get => _teacherUid;
            set => this.RaiseAndSetIfChanged(ref _teacherUid, value);
        }

        /// <summary>
        /// Преподаватель курса
        /// </summary>
        public Teacher? Teacher
        {
            get => _teacher;
            set => this.RaiseAndSetIfChanged(ref _teacher, value);
        }

        /// <summary>
        /// Статус курса
        /// </summary>
        public CourseStatus Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }
    }

    /// <summary>
    /// Модуль курса
    /// </summary>
    public class Module : Base.ViewModelBase
    {
        private Guid _uid;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private int _order;
        private Guid _courseUid;
        private Course _course;

        /// <summary>
        /// Уникальный идентификатор модуля
        /// </summary>
        public new Guid Uid
        {
            get => _uid;
            set => this.RaiseAndSetIfChanged(ref _uid, value);
        }

        /// <summary>
        /// Название модуля
        /// </summary>
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        /// <summary>
        /// Описание модуля
        /// </summary>
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        /// <summary>
        /// Порядковый номер модуля в курсе
        /// </summary>
        public int Order
        {
            get => _order;
            set => this.RaiseAndSetIfChanged(ref _order, value);
        }

        /// <summary>
        /// Идентификатор курса
        /// </summary>
        public Guid CourseUid
        {
            get => _courseUid;
            set => this.RaiseAndSetIfChanged(ref _courseUid, value);
        }

        /// <summary>
        /// Курс, к которому относится модуль
        /// </summary>
        public Course Course
        {
            get => _course;
            set => this.RaiseAndSetIfChanged(ref _course, value);
        }
    }

    /// <summary>
    /// Задание курса
    /// </summary>
    public class Assignment : Base.ViewModelBase
    {
        private Guid _uid;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private DateTime? _dueDate;
        private double _maxScore = 100.0;
        private AssignmentType _type = AssignmentType.Homework;
        private Guid _courseUid;
        private Course? _course;
        private Guid? _lessonUid;
        private Lesson? _lesson;

        /// <summary>
        /// Уникальный идентификатор задания
        /// </summary>
        public new Guid Uid
        {
            get => _uid;
            set => this.RaiseAndSetIfChanged(ref _uid, value);
        }

        /// <summary>
        /// Заголовок задания
        /// </summary>
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        /// <summary>
        /// Описание задания
        /// </summary>
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        /// <summary>
        /// Срок выполнения задания
        /// </summary>
        public DateTime? DueDate
        {
            get => _dueDate;
            set => this.RaiseAndSetIfChanged(ref _dueDate, value);
        }

        /// <summary>
        /// Максимальная оценка за задание
        /// </summary>
        public double MaxScore
        {
            get => _maxScore;
            set => this.RaiseAndSetIfChanged(ref _maxScore, value);
        }

        /// <summary>
        /// Тип задания
        /// </summary>
        public AssignmentType Type
        {
            get => _type;
            set => this.RaiseAndSetIfChanged(ref _type, value);
        }

        /// <summary>
        /// Идентификатор курса
        /// </summary>
        public Guid CourseUid
        {
            get => _courseUid;
            set => this.RaiseAndSetIfChanged(ref _courseUid, value);
        }

        /// <summary>
        /// Курс задания
        /// </summary>
        public Course? Course
        {
            get => _course;
            set => this.RaiseAndSetIfChanged(ref _course, value);
        }

        /// <summary>
        /// Идентификатор урока (если задание привязано к уроку)
        /// </summary>
        public Guid? LessonUid
        {
            get => _lessonUid;
            set => this.RaiseAndSetIfChanged(ref _lessonUid, value);
        }

        /// <summary>
        /// Урок задания
        /// </summary>
        public Lesson? Lesson
        {
            get => _lesson;
            set => this.RaiseAndSetIfChanged(ref _lesson, value);
        }
    }

    /// <summary>
    /// Тип задания
    /// </summary>
    public enum AssignmentType
    {
        /// <summary>
        /// Домашнее задание
        /// </summary>
        Homework,
        
        /// <summary>
        /// Тест
        /// </summary>
        Quiz,
        
        /// <summary>
        /// Экзамен
        /// </summary>
        Exam,
        
        /// <summary>
        /// Проект
        /// </summary>
        Project,
        
        /// <summary>
        /// Лабораторная работа
        /// </summary>
        LabWork
    }

    /// <summary>
    /// Статус курса
    /// </summary>
    public enum CourseStatus
    {
        /// <summary>
        /// Черновик
        /// </summary>
        Draft,
        
        /// <summary>
        /// Опубликован
        /// </summary>
        Published,
        
        /// <summary>
        /// Активен
        /// </summary>
        Active,
        
        /// <summary>
        /// Завершен
        /// </summary>
        Completed,
        
        /// <summary>
        /// Архивирован
        /// </summary>
        Archived
    }

    /// <summary>
    /// Ответ на задание от студента
    /// </summary>
    public class Submission : Base.ViewModelBase
    {
        private Guid _uid;
        private Guid _studentUid;
        private Student? _student;
        private Guid _assignmentUid;
        private Assignment? _assignment;
        private DateTime _submissionDate = DateTime.UtcNow;
        private string _content = string.Empty;
        private double? _score;
        private string _feedback = string.Empty;
        private Guid? _gradedByUid;
        private Teacher? _gradedBy;
        private DateTime? _gradedDate;
        private SubmissionStatus _status = SubmissionStatus.Submitted;

        /// <summary>
        /// Уникальный идентификатор ответа
        /// </summary>
        public new Guid Uid
        {
            get => _uid;
            set => this.RaiseAndSetIfChanged(ref _uid, value);
        }

        /// <summary>
        /// Идентификатор студента
        /// </summary>
        public Guid StudentUid
        {
            get => _studentUid;
            set => this.RaiseAndSetIfChanged(ref _studentUid, value);
        }

        /// <summary>
        /// Студент
        /// </summary>
        public Student? Student
        {
            get => _student;
            set => this.RaiseAndSetIfChanged(ref _student, value);
        }

        /// <summary>
        /// Идентификатор задания
        /// </summary>
        public Guid AssignmentUid
        {
            get => _assignmentUid;
            set => this.RaiseAndSetIfChanged(ref _assignmentUid, value);
        }

        /// <summary>
        /// Задание
        /// </summary>
        public Assignment? Assignment
        {
            get => _assignment;
            set => this.RaiseAndSetIfChanged(ref _assignment, value);
        }

        /// <summary>
        /// Дата отправки ответа
        /// </summary>
        public DateTime SubmissionDate
        {
            get => _submissionDate;
            set => this.RaiseAndSetIfChanged(ref _submissionDate, value);
        }

        /// <summary>
        /// Содержимое ответа
        /// </summary>
        public string Content
        {
            get => _content;
            set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        /// <summary>
        /// Оценка за ответ
        /// </summary>
        public double? Score
        {
            get => _score;
            set => this.RaiseAndSetIfChanged(ref _score, value);
        }

        /// <summary>
        /// Обратная связь от преподавателя
        /// </summary>
        public string Feedback
        {
            get => _feedback;
            set => this.RaiseAndSetIfChanged(ref _feedback, value);
        }

        /// <summary>
        /// Идентификатор преподавателя, выставившего оценку
        /// </summary>
        public Guid? GradedByUid
        {
            get => _gradedByUid;
            set => this.RaiseAndSetIfChanged(ref _gradedByUid, value);
        }

        /// <summary>
        /// Преподаватель, выставивший оценку
        /// </summary>
        public Teacher? GradedBy
        {
            get => _gradedBy;
            set => this.RaiseAndSetIfChanged(ref _gradedBy, value);
        }

        /// <summary>
        /// Дата выставления оценки
        /// </summary>
        public DateTime? GradedDate
        {
            get => _gradedDate;
            set => this.RaiseAndSetIfChanged(ref _gradedDate, value);
        }

        /// <summary>
        /// Статус ответа
        /// </summary>
        public SubmissionStatus Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }
    }

    /// <summary>
    /// Статус ответа на задание
    /// </summary>
    public enum SubmissionStatus
    {
        /// <summary>
        /// Черновик
        /// </summary>
        Draft,
        
        /// <summary>
        /// Отправлено
        /// </summary>
        Submitted,
        
        /// <summary>
        /// Отправлено с опозданием
        /// </summary>
        Late,
        
        /// <summary>
        /// На проверке
        /// </summary>
        UnderReview,
        
        /// <summary>
        /// Оценено
        /// </summary>
        Graded,
        
        /// <summary>
        /// Возвращено на доработку
        /// </summary>
        Returned
    }

    /// <summary>
    /// Зачисление студента на курс
    /// </summary>
    public class Enrollment : Base.ViewModelBase
    {
        private Guid _uid;
        private Guid _studentUid;
        private Student? _student;
        private Guid _courseUid;
        private Course? _course;
        private DateTime _enrollmentDate = DateTime.UtcNow;
        private EnrollmentStatus _status = EnrollmentStatus.Active;

        /// <summary>
        /// Уникальный идентификатор зачисления
        /// </summary>
        public new Guid Uid
        {
            get => _uid;
            set => this.RaiseAndSetIfChanged(ref _uid, value);
        }

        /// <summary>
        /// Идентификатор студента
        /// </summary>
        public Guid StudentUid
        {
            get => _studentUid;
            set => this.RaiseAndSetIfChanged(ref _studentUid, value);
        }

        /// <summary>
        /// Студент
        /// </summary>
        public Student? Student
        {
            get => _student;
            set => this.RaiseAndSetIfChanged(ref _student, value);
        }

        /// <summary>
        /// Идентификатор курса
        /// </summary>
        public Guid CourseUid
        {
            get => _courseUid;
            set => this.RaiseAndSetIfChanged(ref _courseUid, value);
        }

        /// <summary>
        /// Курс
        /// </summary>
        public Course? Course
        {
            get => _course;
            set => this.RaiseAndSetIfChanged(ref _course, value);
        }

        /// <summary>
        /// Дата зачисления
        /// </summary>
        public DateTime EnrollmentDate
        {
            get => _enrollmentDate;
            set => this.RaiseAndSetIfChanged(ref _enrollmentDate, value);
        }

        /// <summary>
        /// Статус зачисления
        /// </summary>
        public EnrollmentStatus Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }
    }

    /// <summary>
    /// Статус зачисления
    /// </summary>
    public enum EnrollmentStatus
    {
        /// <summary>
        /// Активное зачисление
        /// </summary>
        Active,
        
        /// <summary>
        /// Завершено
        /// </summary>
        Completed,
        
        /// <summary>
        /// Отменено
        /// </summary>
        Cancelled,
        
        /// <summary>
        /// Приостановлено
        /// </summary>
        Suspended,
        
        /// <summary>
        /// В ожидании
        /// </summary>
        Pending
    }
}

namespace ViridiscaUi.Infrastructure
{
    /// <summary>
    /// Локальный контекст данных, имитирующий базу данных в памяти
    /// </summary>
    public class LocalDbContext
    {
        // Auth модели
        public SourceList<User> Users { get; } = new SourceList<User>();
        public SourceList<Role> Roles { get; } = new SourceList<Role>();
        public SourceList<Permission> Permissions { get; } = new SourceList<Permission>();
        public SourceList<RolePermission> RolePermissions { get; } = new SourceList<RolePermission>();
        public SourceList<UserRole> UserRoles { get; } = new SourceList<UserRole>();

        // Education модели
        public SourceList<Student> Students { get; } = new SourceList<Student>();
        public SourceList<Group> Groups { get; } = new SourceList<Group>();
        public SourceList<Teacher> Teachers { get; } = new SourceList<Teacher>();
        public SourceList<Course> Courses { get; } = new SourceList<Course>();
        public SourceList<Module> Modules { get; } = new SourceList<Module>();
        public SourceList<Lesson> Lessons { get; } = new SourceList<Lesson>();
        public SourceList<Assignment> Assignments { get; } = new SourceList<Assignment>();
        public SourceList<Submission> Submissions { get; } = new SourceList<Submission>();
        public SourceList<Enrollment> Enrollments { get; } = new SourceList<Enrollment>();

        /// <summary>
        /// Конструктор, инициализирующий данные по умолчанию
        /// </summary>
        public LocalDbContext()
        {
            SeedInitialData();
        }

        /// <summary>
        /// Заполняет начальные данные
        /// </summary>
        private void SeedInitialData()
        {
            // 1. Создаем роли
            var adminRole = new Role
            {
                Uid = Guid.NewGuid(),
                Name = "Administrator",
                Description = "Системный администратор",
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                RoleType = RoleType.SystemAdmin
            };

            var teacherRole = new Role
            {
                Uid = Guid.NewGuid(),
                Name = "Teacher",
                Description = "Преподаватель",
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                RoleType = RoleType.Teacher
            };

            var studentRole = new Role
            {
                Uid = Guid.NewGuid(),
                Name = "Student",
                Description = "Студент",
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                RoleType = RoleType.Student
            };

            Roles.AddRange(new[] { adminRole, teacherRole, studentRole });

            // 2. Создаем админа
            var adminUser = new User
            {
                Uid = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@viridisca.local",
                FirstName = "Admin",
                LastName = "Viridisca",
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            // Добавляем роль администратора
            var adminUserRole = new UserRole
            {
                Uid = Guid.NewGuid(),
                UserUid = adminUser.Uid,
                RoleUid = adminRole.Uid,
                Role = adminRole,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            adminUser.UserRoles.Add(adminUserRole);
            Users.Add(adminUser);
            UserRoles.Add(adminUserRole);

            // 3. Создаем тестового преподавателя
            var teacherUser = new User
            {
                Uid = Guid.NewGuid(),
                Username = "teacher",
                Email = "teacher@viridisca.local",
                FirstName = "Иван",
                LastName = "Преподавателев",
                MiddleName = "Иванович",
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher123"),
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            // Добавляем роль преподавателя
            var teacherUserRole = new UserRole
            {
                Uid = Guid.NewGuid(),
                UserUid = teacherUser.Uid,
                RoleUid = teacherRole.Uid,
                Role = teacherRole,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            teacherUser.UserRoles.Add(teacherUserRole);
            Users.Add(teacherUser);
            UserRoles.Add(teacherUserRole);
            
            // Создаем профиль преподавателя
            var teacher = new Teacher
            {
                Uid = Guid.NewGuid(),
                UserUid = teacherUser.Uid,
                FirstName = teacherUser.FirstName,
                LastName = teacherUser.LastName,
                MiddleName = teacherUser.MiddleName,
                EmployeeCode = "T-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                HireDate = DateTime.UtcNow.AddMonths(-3),
                Status = TeacherStatus.Active,
                AcademicDegree = "Кандидат наук",
                AcademicTitle = "Доцент",
                Specialization = "Информатика",
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            Teachers.Add(teacher);

            // 4. Создаем тестового студента
            var studentUser = new User
            {
                Uid = Guid.NewGuid(),
                Username = "student",
                Email = "student@viridisca.local",
                FirstName = "Петр",
                LastName = "Студентов",
                MiddleName = "Петрович",
                IsActive = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"),
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            // Добавляем роль студента
            var studentUserRole = new UserRole
            {
                Uid = Guid.NewGuid(),
                UserUid = studentUser.Uid,
                RoleUid = studentRole.Uid,
                Role = studentRole,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            studentUser.UserRoles.Add(studentUserRole);
            Users.Add(studentUser);
            UserRoles.Add(studentUserRole);
            
            // 5. Создаем группу
            var group = new Group
            {
                Uid = Guid.NewGuid(),
                Code = "G-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                Name = "Группа 101",
                Description = "Группа информационных технологий",
                Year = DateTime.UtcNow.Year,
                StartDate = new DateTime(DateTime.UtcNow.Year, 9, 1),
                MaxStudents = 25,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            Groups.Add(group);
            
            // 6. Создаем профиль студента
            var student = new Student
            {
                Uid = Guid.NewGuid(),
                UserUid = studentUser.Uid,
                FirstName = studentUser.FirstName,
                LastName = studentUser.LastName,
                MiddleName = studentUser.MiddleName,
                Email = studentUser.Email,
                BirthDate = new DateTime(2000, 1, 15),
                GroupUid = group.Uid,
                Group = group,
                IsActive = true,
                StudentCode = "ST-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                EnrollmentDate = DateTime.UtcNow.AddMonths(-2),
                Status = StudentStatus.Active,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            Students.Add(student);
            
            // 7. Создаем курс
            var course = new Course
            {
                Uid = Guid.NewGuid(),
                Code = "C-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                Name = "Основы программирования",
                Description = "Базовый курс по программированию для начинающих",
                StartDate = DateTime.UtcNow.AddDays(-14),
                EndDate = DateTime.UtcNow.AddMonths(3),
                Credits = 4,
                TeacherUid = teacher.Uid,
                Status = CourseStatus.Active,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            Courses.Add(course);
            
            // 8. Создаем запись о зачислении на курс
            var enrollment = new Enrollment
            {
                Uid = Guid.NewGuid(),
                StudentUid = student.Uid,
                CourseUid = course.Uid,
                EnrollmentDate = DateTime.UtcNow.AddDays(-10),
                Status = EnrollmentStatus.Active,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            
            Enrollments.Add(enrollment);
        }

        #region User CRUD Operations
        /// <summary>
        /// Возвращает текущий список пользователей
        /// </summary>
        public IObservable<IChangeSet<User>> ConnectUsers() => Users.Connect().RefCount();
        
        /// <summary>
        /// Возвращает всех пользователей
        /// </summary>
        public IEnumerable<User> GetAllUsers() => Users.Items;
        
        /// <summary>
        /// Возвращает пользователя по идентификатору
        /// </summary>
        public User? GetUserByUid(Guid uid) => Users.Items.FirstOrDefault(u => u.Uid == uid);
        
        /// <summary>
        /// Возвращает пользователя по имени пользователя
        /// </summary>
        public User? GetUserByUsername(string username) => 
            Users.Items.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        
        /// <summary>
        /// Добавляет нового пользователя
        /// </summary>
        public void AddUser(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.LastModifiedAt = DateTime.UtcNow;
            Users.Add(user);
        }
        
        /// <summary>
        /// Обновляет существующего пользователя
        /// </summary>
        public void UpdateUser(User user)
        {
            var existing = GetUserByUid(user.Uid);
            if (existing != null)
            {
                Users.Remove(existing);
                user.LastModifiedAt = DateTime.UtcNow;
                Users.Add(user);
            }
        }
        
        /// <summary>
        /// Удаляет пользователя
        /// </summary>
        public void DeleteUser(Guid uid)
        {
            var user = GetUserByUid(uid);
            if (user != null)
            {
                Users.Remove(user);
            }
        }
        #endregion

        #region Role CRUD Operations
        /// <summary>
        /// Возвращает текущий список ролей
        /// </summary>
        public IObservable<IChangeSet<Role>> ConnectRoles() => Roles.Connect().RefCount();
        
        /// <summary>
        /// Возвращает все роли
        /// </summary>
        public IEnumerable<Role> GetAllRoles() => Roles.Items;
        
        /// <summary>
        /// Возвращает роль по идентификатору
        /// </summary>
        public Role? GetRoleByUid(Guid uid) => Roles.Items.FirstOrDefault(r => r.Uid == uid);
        
        /// <summary>
        /// Возвращает роль по имени
        /// </summary>
        public Role? GetRoleByName(string name) => 
            Roles.Items.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        
        /// <summary>
        /// Добавляет новую роль
        /// </summary>
        public void AddRole(Role role)
        {
            role.CreatedAt = DateTime.UtcNow;
            role.LastModifiedAt = DateTime.UtcNow;
            Roles.Add(role);
        }
        
        /// <summary>
        /// Обновляет существующую роль
        /// </summary>
        public void UpdateRole(Role role)
        {
            var existing = GetRoleByUid(role.Uid);
            if (existing != null)
            {
                Roles.Remove(existing);
                role.LastModifiedAt = DateTime.UtcNow;
                Roles.Add(role);
            }
        }
        
        /// <summary>
        /// Удаляет роль
        /// </summary>
        public void DeleteRole(Guid uid)
        {
            var role = GetRoleByUid(uid);
            if (role != null)
            {
                Roles.Remove(role);
            }
        }
        #endregion

        #region Student CRUD Operations
        /// <summary>
        /// Возвращает текущий список студентов
        /// </summary>
        public IObservable<IChangeSet<Student>> ConnectStudents() => Students.Connect().RefCount();
        
        /// <summary>
        /// Возвращает всех студентов
        /// </summary>
        public IEnumerable<Student> GetAllStudents() => Students.Items;
        
        /// <summary>
        /// Возвращает студента по идентификатору
        /// </summary>
        public Student? GetStudentByUid(Guid uid) => Students.Items.FirstOrDefault(s => s.Uid == uid);
        
        /// <summary>
        /// Возвращает студентов по идентификатору группы
        /// </summary>
        public IEnumerable<Student> GetStudentsByGroupUid(Guid groupUid) => 
            Students.Items.Where(s => s.GroupUid == groupUid);
        
        /// <summary>
        /// Добавляет нового студента
        /// </summary>
        public void AddStudent(Student student)
        {
            student.CreatedAt = DateTime.UtcNow;
            student.LastModifiedAt = DateTime.UtcNow;
            Students.Add(student);
        }
        
        /// <summary>
        /// Обновляет существующего студента
        /// </summary>
        public void UpdateStudent(Student student)
        {
            var existing = GetStudentByUid(student.Uid);
            if (existing != null)
            {
                Students.Remove(existing);
                student.LastModifiedAt = DateTime.UtcNow;
                Students.Add(student);
            }
        }
        
        /// <summary>
        /// Удаляет студента
        /// </summary>
        public void DeleteStudent(Guid uid)
        {
            var student = GetStudentByUid(uid);
            if (student != null)
            {
                Students.Remove(student);
            }
        }
        #endregion

        #region Group CRUD Operations
        /// <summary>
        /// Возвращает текущий список групп
        /// </summary>
        public IObservable<IChangeSet<Group>> ConnectGroups() => Groups.Connect().RefCount();
        
        /// <summary>
        /// Возвращает все группы
        /// </summary>
        public IEnumerable<Group> GetAllGroups() => Groups.Items;
        
        /// <summary>
        /// Возвращает группу по идентификатору
        /// </summary>
        public Group? GetGroupByUid(Guid uid) => Groups.Items.FirstOrDefault(g => g.Uid == uid);
        
        /// <summary>
        /// Добавляет новую группу
        /// </summary>
        public void AddGroup(Group group)
        {
            group.CreatedAt = DateTime.UtcNow;
            group.LastModifiedAt = DateTime.UtcNow;
            Groups.Add(group);
        }
        
        /// <summary>
        /// Обновляет существующую группу
        /// </summary>
        public void UpdateGroup(Group group)
        {
            var existing = GetGroupByUid(group.Uid);
            if (existing != null)
            {
                Groups.Remove(existing);
                group.LastModifiedAt = DateTime.UtcNow;
                Groups.Add(group);
            }
        }
        
        /// <summary>
        /// Удаляет группу
        /// </summary>
        public void DeleteGroup(Guid uid)
        {
            var group = GetGroupByUid(uid);
            if (group != null)
            {
                Groups.Remove(group);
            }
        }
        #endregion

        #region Teacher CRUD Operations
        /// <summary>
        /// Возвращает текущий список преподавателей
        /// </summary>
        public IObservable<IChangeSet<Teacher>> ConnectTeachers() => Teachers.Connect().RefCount();
        
        /// <summary>
        /// Возвращает всех преподавателей
        /// </summary>
        public IEnumerable<Teacher> GetAllTeachers() => Teachers.Items;
        
        /// <summary>
        /// Возвращает преподавателя по идентификатору
        /// </summary>
        public Teacher? GetTeacherByUid(Guid uid) => Teachers.Items.FirstOrDefault(t => t.Uid == uid);
        
        /// <summary>
        /// Добавляет нового преподавателя
        /// </summary>
        public void AddTeacher(Teacher teacher)
        {
            teacher.CreatedAt = DateTime.UtcNow;
            teacher.LastModifiedAt = DateTime.UtcNow;
            Teachers.Add(teacher);
        }
        
        /// <summary>
        /// Обновляет существующего преподавателя
        /// </summary>
        public void UpdateTeacher(Teacher teacher)
        {
            var existing = GetTeacherByUid(teacher.Uid);
            if (existing != null)
            {
                Teachers.Remove(existing);
                teacher.LastModifiedAt = DateTime.UtcNow;
                Teachers.Add(teacher);
            }
        }
        
        /// <summary>
        /// Удаляет преподавателя
        /// </summary>
        public void DeleteTeacher(Guid uid)
        {
            var teacher = GetTeacherByUid(uid);
            if (teacher != null)
            {
                Teachers.Remove(teacher);
            }
        }
        #endregion

        #region Course CRUD Operations
        /// <summary>
        /// Возвращает текущий список курсов
        /// </summary>
        public IObservable<IChangeSet<Course>> ConnectCourses() => Courses.Connect().RefCount();
        
        /// <summary>
        /// Возвращает все курсы
        /// </summary>
        public IEnumerable<Course> GetAllCourses() => Courses.Items;
        
        /// <summary>
        /// Возвращает курс по идентификатору
        /// </summary>
        public Course? GetCourseByUid(Guid uid) => Courses.Items.FirstOrDefault(c => c.Uid == uid);
        
        /// <summary>
        /// Возвращает курсы по идентификатору преподавателя
        /// </summary>
        public IEnumerable<Course> GetCoursesByTeacherUid(Guid teacherUid) => 
            Courses.Items.Where(c => c.TeacherUid == teacherUid);
        
        /// <summary>
        /// Добавляет новый курс
        /// </summary>
        public void AddCourse(Course course)
        {
            course.CreatedAt = DateTime.UtcNow;
            course.LastModifiedAt = DateTime.UtcNow;
            Courses.Add(course);
        }
        
        /// <summary>
        /// Обновляет существующий курс
        /// </summary>
        public void UpdateCourse(Course course)
        {
            var existing = GetCourseByUid(course.Uid);
            if (existing != null)
            {
                Courses.Remove(existing);
                course.LastModifiedAt = DateTime.UtcNow;
                Courses.Add(course);
            }
        }
        
        /// <summary>
        /// Удаляет курс
        /// </summary>
        public void DeleteCourse(Guid uid)
        {
            var course = GetCourseByUid(uid);
            if (course != null)
            {
                Courses.Remove(course);
            }
        }
        #endregion

        #region Enrollment CRUD Operations
        /// <summary>
        /// Возвращает текущий список зачислений
        /// </summary>
        public IObservable<IChangeSet<Enrollment>> ConnectEnrollments() => Enrollments.Connect().RefCount();
        
        /// <summary>
        /// Возвращает все зачисления
        /// </summary>
        public IEnumerable<Enrollment> GetAllEnrollments() => Enrollments.Items;
        
        /// <summary>
        /// Возвращает зачисление по идентификатору
        /// </summary>
        public Enrollment? GetEnrollmentByUid(Guid uid) => Enrollments.Items.FirstOrDefault(e => e.Uid == uid);
        
        /// <summary>
        /// Возвращает зачисления по идентификатору студента
        /// </summary>
        public IEnumerable<Enrollment> GetEnrollmentsByStudentUid(Guid studentUid) => 
            Enrollments.Items.Where(e => e.StudentUid == studentUid);
        
        /// <summary>
        /// Возвращает зачисления по идентификатору курса
        /// </summary>
        public IEnumerable<Enrollment> GetEnrollmentsByCourseUid(Guid courseUid) => 
            Enrollments.Items.Where(e => e.CourseUid == courseUid);
        
        /// <summary>
        /// Добавляет новое зачисление
        /// </summary>
        public void AddEnrollment(Enrollment enrollment)
        {
            enrollment.CreatedAt = DateTime.UtcNow;
            enrollment.LastModifiedAt = DateTime.UtcNow;
            Enrollments.Add(enrollment);
        }
        
        /// <summary>
        /// Обновляет существующее зачисление
        /// </summary>
        public void UpdateEnrollment(Enrollment enrollment)
        {
            var existing = GetEnrollmentByUid(enrollment.Uid);
            if (existing != null)
            {
                Enrollments.Remove(existing);
                enrollment.LastModifiedAt = DateTime.UtcNow;
                Enrollments.Add(enrollment);
            }
        }
        
        /// <summary>
        /// Удаляет зачисление
        /// </summary>
        public void DeleteEnrollment(Guid uid)
        {
            var enrollment = GetEnrollmentByUid(uid);
            if (enrollment != null)
            {
                Enrollments.Remove(enrollment);
            }
        }
        #endregion

        #region UserRole CRUD Operations
        /// <summary>
        /// Возвращает текущий список связей пользователей с ролями
        /// </summary>
        public IObservable<IChangeSet<UserRole>> ConnectUserRoles() => UserRoles.Connect().RefCount();
        
        /// <summary>
        /// Возвращает все связи пользователей с ролями
        /// </summary>
        public IEnumerable<UserRole> GetAllUserRoles() => UserRoles.Items;
        
        /// <summary>
        /// Возвращает связь пользователя с ролью по идентификатору
        /// </summary>
        public UserRole? GetUserRoleByUid(Guid uid) => UserRoles.Items.FirstOrDefault(ur => ur.Uid == uid);
        
        /// <summary>
        /// Возвращает связи для указанного пользователя
        /// </summary>
        public IEnumerable<UserRole> GetUserRolesByUserUid(Guid userUid) => 
            UserRoles.Items.Where(ur => ur.UserUid == userUid);
        
        /// <summary>
        /// Возвращает связи для указанной роли
        /// </summary>
        public IEnumerable<UserRole> GetUserRolesByRoleUid(Guid roleUid) => 
            UserRoles.Items.Where(ur => ur.RoleUid == roleUid);
        
        /// <summary>
        /// Добавляет новую связь пользователя с ролью
        /// </summary>
        public void AddUserRole(UserRole userRole)
        {
            userRole.CreatedAt = DateTime.UtcNow;
            userRole.LastModifiedAt = DateTime.UtcNow;
            UserRoles.Add(userRole);
        }
        
        /// <summary>
        /// Обновляет существующую связь пользователя с ролью
        /// </summary>
        public void UpdateUserRole(UserRole userRole)
        {
            var existing = GetUserRoleByUid(userRole.Uid);
            if (existing != null)
            {
                UserRoles.Remove(existing);
                userRole.LastModifiedAt = DateTime.UtcNow;
                UserRoles.Add(userRole);
            }
        }
        
        /// <summary>
        /// Удаляет связь пользователя с ролью
        /// </summary>
        public void DeleteUserRole(Guid uid)
        {
            var userRole = GetUserRoleByUid(uid);
            if (userRole != null)
            {
                UserRoles.Remove(userRole);
            }
        }
        #endregion
    }
} 