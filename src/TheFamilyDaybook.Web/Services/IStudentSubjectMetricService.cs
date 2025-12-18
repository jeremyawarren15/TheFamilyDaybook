using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public interface IStudentSubjectMetricService
{
    Task<IEnumerable<StudentSubjectMetricConfigModel>> GetAvailableMetricsForStudentSubjectAsync(int studentId, int subjectId, int familyId);
    Task<IEnumerable<Metric>> GetMetricsForDailyLogAsync(int studentId, int subjectId, int familyId);
    Task<StudentServiceResult> SaveStudentSubjectMetricConfigAsync(int studentId, int subjectId, List<StudentSubjectMetricConfigModel> configs);
}

