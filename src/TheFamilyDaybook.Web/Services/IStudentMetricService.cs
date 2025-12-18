using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public interface IStudentMetricService
{
    Task<IEnumerable<StudentMetricConfigModel>> GetMetricsForStudentAsync(int studentId, int familyId);
    Task<IEnumerable<StudentMetricConfigViewModel>> GetMetricConfigurationsForStudentAsync(int studentId);
    Task<StudentServiceResult> SaveStudentMetricConfigAsync(int studentId, List<StudentMetricConfigModel> configs);
    Task<StudentServiceResult> UpdateStudentMetricAsync(int studentId, int metricId, bool isEnabled, bool appliesToAllSubjects);
    Task<StudentServiceResult> EnableMetricForStudentAsync(int studentId, int metricId, bool appliesToAllSubjects);
    Task<StudentServiceResult> DisableMetricForStudentAsync(int studentId, int metricId);
}

