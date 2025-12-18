using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public interface IMetricService
{
    Task<IEnumerable<Metric>> GetAllMetricsAsync(int familyId);
    Task<IEnumerable<Metric>> GetTemplateMetricsAsync();
    Task<IEnumerable<Metric>> GetCustomMetricsByFamilyIdAsync(int familyId);
    Task<Metric?> GetMetricByIdAsync(int metricId);
    Task<MetricServiceResult> CreateMetricAsync(int familyId, MetricModel model);
    Task<MetricServiceResult> UpdateMetricAsync(int metricId, MetricModel model);
    Task<MetricServiceResult> DeleteMetricAsync(int metricId);
}

