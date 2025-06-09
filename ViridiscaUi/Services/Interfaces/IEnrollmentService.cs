using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для управления записями студентов на курсы
/// </summary>
public interface IEnrollmentService
{
    /// <summary>
    /// Получить все записи
    /// </summary>
    Task<IEnumerable<Enrollment>> GetAllAsync();

    /// <summary>
    /// Получить запись по Uid
    /// </summary>
    Task<Enrollment?> GetByUidAsync(Guid uid);

    /// <summary>
    /// Получить записи студента
    /// </summary>
    Task<IEnumerable<Enrollment>> GetByStudentUidAsync(Guid studentUid);

    /// <summary>
    /// Получить записи на курс
    /// </summary>
    Task<IEnumerable<Enrollment>> GetByCourseInstanceUidAsync(Guid courseInstanceUid);

    /// <summary>
    /// Создать новую запись
    /// </summary>
    Task<Enrollment> CreateAsync(Enrollment enrollment);

    /// <summary>
    /// Обновить запись
    /// </summary>
    Task<Enrollment> UpdateAsync(Enrollment enrollment);

    /// <summary>
    /// Удалить запись
    /// </summary>
    Task DeleteAsync(Guid uid);

    /// <summary>
    /// Проверить существование записи
    /// </summary>
    Task<bool> ExistsAsync(Guid uid);

    /// <summary>
    /// Записать студента на курс
    /// </summary>
    Task<Enrollment> EnrollStudentAsync(Guid studentUid, Guid courseInstanceUid);

    /// <summary>
    /// Отчислить студента с курса
    /// </summary>
    Task UnenrollStudentAsync(Guid studentUid, Guid courseInstanceUid);

    /// <summary>
    /// Получить записи по статусу
    /// </summary>
    Task<IEnumerable<Enrollment>> GetByStatusAsync(EnrollmentStatus status);

    /// <summary>
    /// Обновить статус записи
    /// </summary>
    Task UpdateStatusAsync(Guid enrollmentUid, EnrollmentStatus status);

    /// <summary>
    /// Получить активные записи студента
    /// </summary>
    Task<IEnumerable<Enrollment>> GetActiveEnrollmentsByStudentAsync(Guid studentUid);

    /// <summary>
    /// Получить количество записей на курс
    /// </summary>
    Task<int> GetEnrollmentCountByCourseAsync(Guid courseInstanceUid);

    /// <summary>
    /// Проверить, записан ли студент на курс
    /// </summary>
    Task<bool> IsStudentEnrolledAsync(Guid studentUid, Guid courseInstanceUid);
} 