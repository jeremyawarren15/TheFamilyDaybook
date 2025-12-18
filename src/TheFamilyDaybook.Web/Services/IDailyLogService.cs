using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public interface IDailyLogService
{
    Task<DailyLog?> GetDailyLogByIdAsync(int dailyLogId);
    Task<DailyLog?> GetDailyLogAsync(int studentId, int subjectId, DateTime date);
    Task<IEnumerable<DailyLog>> GetDailyLogsForStudentAsync(int studentId);
    Task<IEnumerable<DailyLog>> GetDailyLogsForStudentSubjectAsync(int studentId, int subjectId);
    Task<IEnumerable<Metric>> GetAvailableMetricsForDailyLogAsync(int studentId, int subjectId, int familyId);
    Task<DailyLogServiceResult> CreateDailyLogAsync(DailyLogModel model);
    Task<DailyLogServiceResult> UpdateDailyLogAsync(int dailyLogId, DailyLogModel model);
    Task<DailyLogServiceResult> DeleteDailyLogAsync(int dailyLogId);
}

