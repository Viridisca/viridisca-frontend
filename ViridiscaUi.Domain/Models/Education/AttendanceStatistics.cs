namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Статистика посещаемости
/// </summary>
public class AttendanceStatistics
{
    public int TotalLessons { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int ExcusedCount { get; set; }
    public double AttendanceRate => TotalLessons > 0 ? (double)PresentCount / TotalLessons * 100 : 0;
} 