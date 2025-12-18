using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public interface IStudentMetricService
{
    Task<IEnumerable<StudentMetricConfigModel>> GetMetricsForStudentAsync(int studentId, int familyId);
    Task<StudentServiceResult> SaveStudentMetricConfigAsync(int studentId, List<StudentMetricConfigModel> configs);
    Task<StudentServiceResult> UpdateStudentMetricAsync(int studentId, int metricId, bool isEnabled, bool appliesToAllSubjects);
}

