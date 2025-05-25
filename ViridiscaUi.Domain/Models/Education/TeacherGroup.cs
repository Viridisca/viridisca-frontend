using System;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Education
{
    /// <summary>
    /// Связь преподавателя с группой
    /// </summary>
    public class TeacherGroup : ViewModelBase
    {
        private Guid _teacherUid;
        private Guid _groupUid;
        private Guid _subjectUid;
        private bool _isCurator;
        private DateTime _assignedAt;
        private DateTime? _endedAt;
        private bool _isActive;

        /// <summary>
        /// Идентификатор преподавателя
        /// </summary>
        public Guid TeacherUid
        {
            get => _teacherUid;
            set => this.RaiseAndSetIfChanged(ref _teacherUid, value);
        }

        /// <summary>
        /// Идентификатор группы
        /// </summary>
        public Guid GroupUid
        {
            get => _groupUid;
            set => this.RaiseAndSetIfChanged(ref _groupUid, value);
        }

        /// <summary>
        /// Идентификатор предмета
        /// </summary>
        public Guid SubjectUid
        {
            get => _subjectUid;
            set => this.RaiseAndSetIfChanged(ref _subjectUid, value);
        }

        /// <summary>
        /// Флаг куратора группы
        /// </summary>
        public bool IsCurator
        {
            get => _isCurator;
            set => this.RaiseAndSetIfChanged(ref _isCurator, value);
        }

        /// <summary>
        /// Дата назначения
        /// </summary>
        public DateTime AssignedAt
        {
            get => _assignedAt;
            set => this.RaiseAndSetIfChanged(ref _assignedAt, value);
        }

        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime? EndedAt
        {
            get => _endedAt;
            set => this.RaiseAndSetIfChanged(ref _endedAt, value);
        }

        /// <summary>
        /// Флаг активности связи
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        /// <summary>
        /// Ссылка на объект преподавателя
        /// </summary>
        public required Teacher Teacher { get; set; }

        /// <summary>
        /// Ссылка на объект группы
        /// </summary>
        public required Group Group { get; set; }

        /// <summary>
        /// Ссылка на объект предмета
        /// </summary>
        public required Subject Subject { get; set; }

        /// <summary>
        /// Создает новый экземпляр связи преподавателя с группой
        /// </summary>
        public TeacherGroup()
        {
            _isActive = true;
            _isCurator = false;
            _assignedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Создает новый экземпляр связи преподавателя с группой с указанными параметрами
        /// </summary>
        public TeacherGroup(Guid teacherUid, Guid groupUid, Guid subjectUid, bool isCurator = false)
        {
            Uid = Guid.NewGuid();
            _teacherUid = teacherUid;
            _groupUid = groupUid;
            _subjectUid = subjectUid;
            _isCurator = isCurator;
            _isActive = true;
            _assignedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Устанавливает флаг куратора
        /// </summary>
        public void SetAsCurator(bool isCurator)
        {
            IsCurator = isCurator;
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Завершает связь преподавателя с группой
        /// </summary>
        public void End(DateTime endDate)
        {
            if (endDate <= AssignedAt)
                return;

            EndedAt = endDate;
            IsActive = false;
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Активирует связь преподавателя с группой
        /// </summary>
        public void Activate()
        {
            IsActive = true;
            EndedAt = null;
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Деактивирует связь преподавателя с группой
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            if (!EndedAt.HasValue)
            {
                EndedAt = DateTime.UtcNow;
            }
            LastModifiedAt = DateTime.UtcNow;
        }
    }
} 