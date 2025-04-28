using System;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education
{
    /// <summary>
    /// Детальная информация об уроке
    /// </summary>
    public class LessonDetail : ViewModelBase
    {
        private Guid _lessonUid;
        private string _topic = string.Empty;
        private string _description = string.Empty;
        private DateTime _startTime;
        private DateTime _endTime;
        private string _teacherFirstName = string.Empty;
        private string _teacherLastName = string.Empty;
        private string _teacherMiddleName = string.Empty;
        private string _subjectName = string.Empty;
        private string _groupName = string.Empty;
        private bool _isCancelled;
        private bool _isCompleted;

        /// <summary>
        /// Идентификатор урока
        /// </summary>
        public Guid LessonUid
        {
            get => _lessonUid;
            set => this.RaiseAndSetIfChanged(ref _lessonUid, value);
        }

        /// <summary>
        /// Тема урока
        /// </summary>
        public string Topic
        {
            get => _topic;
            set => this.RaiseAndSetIfChanged(ref _topic, value);
        }

        /// <summary>
        /// Описание урока
        /// </summary>
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        /// <summary>
        /// Время начала урока
        /// </summary>
        public DateTime StartTime
        {
            get => _startTime;
            set => this.RaiseAndSetIfChanged(ref _startTime, value);
        }

        /// <summary>
        /// Время окончания урока
        /// </summary>
        public DateTime EndTime
        {
            get => _endTime;
            set => this.RaiseAndSetIfChanged(ref _endTime, value);
        }

        /// <summary>
        /// Имя преподавателя
        /// </summary>
        public string TeacherFirstName
        {
            get => _teacherFirstName;
            set => this.RaiseAndSetIfChanged(ref _teacherFirstName, value);
        }

        /// <summary>
        /// Фамилия преподавателя
        /// </summary>
        public string TeacherLastName
        {
            get => _teacherLastName;
            set => this.RaiseAndSetIfChanged(ref _teacherLastName, value);
        }

        /// <summary>
        /// Отчество преподавателя
        /// </summary>
        public string TeacherMiddleName
        {
            get => _teacherMiddleName;
            set => this.RaiseAndSetIfChanged(ref _teacherMiddleName, value);
        }

        /// <summary>
        /// Название предмета
        /// </summary>
        public string SubjectName
        {
            get => _subjectName;
            set => this.RaiseAndSetIfChanged(ref _subjectName, value);
        }

        /// <summary>
        /// Название группы
        /// </summary>
        public string GroupName
        {
            get => _groupName;
            set => this.RaiseAndSetIfChanged(ref _groupName, value);
        }

        /// <summary>
        /// Признак отмены урока
        /// </summary>
        public bool IsCancelled
        {
            get => _isCancelled;
            set => this.RaiseAndSetIfChanged(ref _isCancelled, value);
        }

        /// <summary>
        /// Признак завершения урока
        /// </summary>
        public bool IsCompleted
        {
            get => _isCompleted;
            set => this.RaiseAndSetIfChanged(ref _isCompleted, value);
        }

        /// <summary>
        /// Полное имя преподавателя (Фамилия Имя Отчество)
        /// </summary>
        public string FullName => $"{TeacherLastName} {TeacherFirstName} {TeacherMiddleName}".Trim();

        /// <summary>
        /// Продолжительность урока в минутах
        /// </summary>
        public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;

        /// <summary>
        /// Создает новый экземпляр детальной информации об уроке
        /// </summary>
        public LessonDetail()
        {
        }

        /// <summary>
        /// Создает новый экземпляр детальной информации об уроке с указанными параметрами
        /// </summary>
        public LessonDetail(
            Guid lessonUid,
            string topic,
            DateTime startTime,
            DateTime endTime,
            string teacherLastName,
            string teacherFirstName,
            string teacherMiddleName,
            string subjectName,
            string groupName,
            string description = "",
            bool isCancelled = false,
            bool isCompleted = false)
        {
            Uid = Guid.NewGuid();
            _lessonUid = lessonUid;
            _topic = topic;
            _startTime = startTime;
            _endTime = endTime;
            _teacherLastName = teacherLastName;
            _teacherFirstName = teacherFirstName;
            _teacherMiddleName = teacherMiddleName ?? string.Empty;
            _subjectName = subjectName;
            _groupName = groupName;
            _description = description;
            _isCancelled = isCancelled;
            _isCompleted = isCompleted;
        }

        /// <summary>
        /// Создает детальную информацию об уроке на основе объекта урока
        /// </summary>
        public static LessonDetail FromLesson(Lesson lesson)
        {
            if (lesson == null)
                throw new ArgumentNullException(nameof(lesson));

            if (lesson.Teacher == null || lesson.Subject == null || lesson.Group == null)
                throw new InvalidOperationException("Для создания детальной информации об уроке необходимы данные о преподавателе, предмете и группе");

            return new LessonDetail(
                lesson.Uid,
                lesson.Topic,
                lesson.StartTime,
                lesson.EndTime,
                lesson.Teacher.LastName,
                lesson.Teacher.FirstName,
                lesson.Teacher.MiddleName,
                lesson.Subject.Name,
                lesson.Group.Name,
                lesson.Description,
                lesson.IsCancelled,
                lesson.IsCompleted);
        }
    }
} 