using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.ViewModels.Education;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы с оценками
/// </summary>
public interface IGradeService
{
    /// <summary>
    /// Получает оценку по идентификатору
    /// </summary>
    Task<Grade?> GetGradeAsync(Guid uid);
    
    /// <summary>
    /// Получает все оценки
    /// </summary>
    Task<IEnumerable<Grade>> GetAllGradesAsync(
        Guid? courseUid = null,
        Guid? groupUid = null,
        (decimal? min, decimal? max)? gradeRange = null,
        (DateTime? start, DateTime? end)? period = null);
    
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
    Task<GradeStatistics> GetGradeStatisticsAsync(
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
}
