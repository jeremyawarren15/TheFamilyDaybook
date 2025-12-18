using TheFamilyDaybook.Models;

namespace TheFamilyDaybook.Web.ViewModels;

public class StudentMetricConfigModel
{
    public int MetricId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public MetricType MetricType { get; set; }
    public string? Category { get; set; }
    public bool IsEnabled { get; set; }
    public bool AppliesToAllSubjects { get; set; } = true;
}

