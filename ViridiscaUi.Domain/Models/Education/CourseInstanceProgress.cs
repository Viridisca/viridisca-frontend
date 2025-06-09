using System;

namespace ViridiscaUi.Domain.Models.Education
{
    /// <summary>
    /// Прогресс студента по экземпляру курса
    /// </summary>
    public class CourseInstanceProgress
    {
        /// <summary>
        /// Идентификатор экземпляра курса
        /// </summary>
        public Guid CourseInstanceUid { get; set; }

        /// <summary>
        /// Идентификатор студента
        /// </summary>
        public Guid StudentUid { get; set; }

        /// <summary>
        /// Количество завершенных занятий
        /// </summary>
        public int CompletedLessons { get; set; }

        /// <summary>
        /// Общее количество занятий
        /// </summary>
        public int TotalLessons { get; set; }

        /// <summary>
        /// Количество выполненных заданий
        /// </summary>
        public int CompletedAssignments { get; set; }

        /// <summary>
        /// Общее количество заданий
        /// </summary>
        public int TotalAssignments { get; set; }

        /// <summary>
        /// Средняя оценка
        /// </summary>
        public double AverageGrade { get; set; }

        /// <summary>
        /// Процент завершения курса
        /// </summary>
        public double CompletionPercentage { get; set; }

        /// <summary>
        /// Дата последней активности
        /// </summary>
        public DateTime? LastActivityDate { get; set; }

        /// <summary>
        /// Дата записи на курс
        /// </summary>
        public DateTime EnrolledAt { get; set; }

        /// <summary>
        /// Общее время, потраченное на курс
        /// </summary>
        public TimeSpan TotalTimeSpent { get; set; }
    }
} 