using TheFamilyDaybook.Models;

namespace TheFamilyDaybook.Web.ViewModels;

public class StudentSubjectMetricConfigModel
{
    public int MetricId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public MetricType MetricType { get; set; }
    public string? Category { get; set; }
    public bool IsEnabled { get; set; }
    public bool AppliesToAllSubjects { get; set; } // Indicates if this metric applies to all subjects at student level
}

