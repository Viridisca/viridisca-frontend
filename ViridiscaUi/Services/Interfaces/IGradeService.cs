using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Education.Enums;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы с оценками
/// Наследуется от IGenericCrudService для получения универсальных CRUD операций
/// </summary>
public interface IGradeService : IGenericCrudService<Grade>
{
    /// <summary>
    /// Получает оценку по идентификатору
    /// </summary>
    Task<Grade?> GetGradeAsync(Guid uid);
    
    /// <summary>
    /// Получает все оценки
    /// </summary>
    Task<IEnumerable<Grade>> GetAllGradesAsync();
    
    /// <summary>
    /// Получает все оценки (алиас для GetAllGradesAsync)
    /// </summary>
    Task<IEnumerable<Grade>> GetGradesAsync();
    
    /// <summary>
    /// Создает новую оценку
    /// </summary>
    Task<Grade> CreateGradeAsync(Grade grade);
    
    /// <summary>
    /// Добавляет новую оценку
    /// </summary>
    Task AddGradeAsync(Grade grade);
    
    /// <summary>
    /// Обновляет существующую оценку
    /// </summary>
    Task<bool> UpdateGradeAsync(Grade grade);
    
    /// <summary>
    /// Удаляет оценку
    /// </summary>
    Task<bool> DeleteGradeAsync(Guid uid);
    
    /// <summary>
    /// Получает оценки по студенту
    /// </summary>
    Task<IEnumerable<Grade>> GetGradesByStudentAsync(Guid studentUid);
    
    /// <summary>
    /// Получает оценки по заданию
    /// </summary>
    Task<IEnumerable<Grade>> GetGradesByAssignmentAsync(Guid assignmentUid);
    
    /// <summary>
    /// Получает оценки по преподавателю
    /// </summary>
    Task<IEnumerable<Grade>> GetGradesByTeacherAsync(Guid teacherUid);
    
    /// <summary>
    /// Получает оценки по курсу
    /// </summary>
    Task<IEnumerable<Grade>> GetGradesByCourseAsync(Guid courseUid);
    
    /// <summary>
    /// Получает оценки с пагинацией
    /// </summary>
    Task<(IEnumerable<Grade> Grades, int TotalCount)> GetGradesPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        Guid? courseUid = null,
        Guid? groupUid = null,
        (decimal? min, decimal? max)? gradeRange = null,
        (DateTime? start, DateTime? end)? period = null);
    
    /// <summary>
    /// Обновляет комментарий к оценке
    /// </summary>
    Task<bool> UpdateGradeCommentAsync(Guid gradeUid, string comment);
    
    /// <summary>
    /// Массовое добавление оценок
    /// </summary>
    Task<bool> BulkAddGradesAsync(IEnumerable<Grade> grades);
    
    /// <summary>
    /// Получает статистику оценок
    /// </summary>
    Task<ViewModels.Education.GradeStatistics> GetGradeStatisticsAsync(
        Guid? courseUid = null,
        Guid? groupUid = null,
        (DateTime? start, DateTime? end)? period = null);
    
    /// <summary>
    /// Получает недавние оценки
    /// </summary>
    Task<IEnumerable<Grade>> GetRecentGradesAsync(int days);
    
    /// <summary>
    /// Генерирует аналитический отчет по оценкам
    /// </summary>
    Task<object> GenerateAnalyticsReportAsync(
        Guid? courseUid = null,
        Guid? groupUid = null,
        (DateTime? start, DateTime? end)? period = null);
    
    /// <summary>
    /// Получает оценки студента
    /// </summary>
    Task<IEnumerable<Grade>> GetStudentGradesAsync(Guid studentUid);
    
    /// <summary>
    /// Получает оценки по курсу
    /// </summary>
    Task<IEnumerable<Grade>> GetCourseGradesAsync(Guid courseUid);
    
    /// <summary>
    /// Получает оценки по заданию
    /// </summary>
    Task<IEnumerable<Grade>> GetAssignmentGradesAsync(Guid assignmentUid);
    
    /// <summary>
    /// Получает оценки с пагинацией и фильтрацией
    /// </summary>
    Task<(IEnumerable<Grade> grades, int totalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        Guid? studentUid = null,
        Guid? courseInstanceUid = null,
        Guid? assignmentUid = null,
        GradeType? gradeType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    
    /// <summary>
    /// Получает статистику оценок
    /// </summary>
    Task<object> GetStatisticsAsync(Guid? studentUid = null, Guid? courseInstanceUid = null);
    
    /// <summary>
    /// Получает оценку по студенту и заданию
    /// </summary>
    Task<Grade?> GetByStudentAndAssignmentAsync(Guid studentUid, Guid assignmentUid);
}
