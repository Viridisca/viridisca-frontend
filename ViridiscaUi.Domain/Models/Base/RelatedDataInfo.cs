using System;
using System.Collections.Generic;

namespace ViridiscaUi.Domain.Models.Base;

/// <summary>
/// Базовый класс для информации о связанных данных
/// </summary>
public abstract class RelatedDataInfo
{
    public virtual bool HasRelatedData { get; set; }
    public virtual List<string> RelatedDataDescriptions { get; set; } = new();
    
    public virtual string GetWarningMessage()
    {
        if (RelatedDataDescriptions.Count > 0)
        {
            return string.Join("\n", RelatedDataDescriptions);
        }
        return string.Empty;
    }
}

/// <summary>
/// Информация о связанных данных группы
/// </summary>
public class GroupRelatedDataInfo : RelatedDataInfo
{
    public bool HasStudents { get; set; }
    public int StudentsCount { get; set; }
    
    public bool HasCourseInstances { get; set; }
    public int CourseInstancesCount { get; set; }
    
    public bool HasSchedule { get; set; }
    public int ScheduleCount { get; set; }
}

/// <summary>
/// Информация о связанных данных предмета
/// </summary>
public class SubjectRelatedDataInfo : RelatedDataInfo
{
    public bool HasCourseInstances { get; set; }
    public int CourseInstancesCount { get; set; }
    
    public bool HasAssignments { get; set; }
    public int AssignmentsCount { get; set; }
    
    public bool HasGrades { get; set; }
    public int GradesCount { get; set; }

    public bool HasCurriculumSubjects { get; set; }
    public int CurriculumSubjectsCount { get; set; }

    // Aliases for compatibility with ViewModels
    public int CoursesCount => CourseInstancesCount;
    public int CurriculaCount => CurriculumSubjectsCount;
}

/// <summary>
/// Информация о связанных данных преподавателя
/// </summary>
public class TeacherRelatedDataInfo : RelatedDataInfo
{
    public bool HasCourseInstances { get; set; }
    public int CourseInstancesCount { get; set; }
    
    public bool HasGroups { get; set; }
    public int GroupsCount { get; set; }
    
    public bool HasSchedule { get; set; }
    public int ScheduleCount { get; set; }

    public bool HasGrades { get; set; }
    public int GradesCount { get; set; }

    public bool HasAssignments { get; set; }
    public int AssignmentsCount { get; set; }

    public bool HasCuratedGroups { get; set; }
    public int CuratedGroupsCount { get; set; }
}

/// <summary>
/// Информация о связанных данных студента
/// </summary>
public class StudentRelatedDataInfo : RelatedDataInfo
{
    public bool HasEnrollments { get; set; }
    public int EnrollmentsCount { get; set; }
    
    public bool HasGrades { get; set; }
    public int GradesCount { get; set; }
    
    public bool HasSubmissions { get; set; }
    public int SubmissionsCount { get; set; }
    
    public bool HasAttendance { get; set; }
    public int AttendanceCount { get; set; }

    public bool HasAssignments { get; set; }
    public int AssignmentsCount { get; set; }

    public bool HasCourses { get; set; }
    public int CoursesCount { get; set; }
}

/// <summary>
/// Информация о связанных данных задания
/// </summary>
public class AssignmentRelatedDataInfo : RelatedDataInfo
{
    public bool HasSubmissions { get; set; }
    public int SubmissionsCount { get; set; }
    
    public bool HasGrades { get; set; }
    public int GradesCount { get; set; }
}

/// <summary>
/// Информация о связанных данных экзамена
/// </summary>
public class ExamRelatedDataInfo : RelatedDataInfo
{
    public bool HasResults { get; set; }
    public int ResultsCount { get; set; }

    public bool HasSubmissions { get; set; }
    public int SubmissionsCount { get; set; }
}

/// <summary>
/// Информация о связанных данных учебного плана
/// </summary>
public class CurriculumRelatedDataInfo : RelatedDataInfo
{
    public bool HasSubjects { get; set; }
    public int SubjectsCount { get; set; }

    public bool HasStudents { get; set; }
    public int StudentsCount { get; set; }
}

/// <summary>
/// Информация о связанных данных слота расписания
/// </summary>
public class ScheduleSlotRelatedDataInfo : RelatedDataInfo
{
    public bool HasAttendance { get; set; }
    public int AttendanceCount { get; set; }

    public bool HasConflicts { get; set; }
    public int ConflictsCount { get; set; }
}

/// <summary>
/// Информация о связанных данных библиотечного ресурса
/// </summary>
public class LibraryResourceRelatedDataInfo : RelatedDataInfo
{
    public bool HasActiveLoans { get; set; }
    public int ActiveLoansCount { get; set; }

    public bool HasLoans { get; set; }
    public int LoansCount { get; set; }
} 