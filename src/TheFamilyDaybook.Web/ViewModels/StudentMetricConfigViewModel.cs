using TheFamilyDaybook.Models;

namespace TheFamilyDaybook.Web.ViewModels;

public class StudentMetricConfigViewModel
{
    public int MetricId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public MetricType MetricType { get; set; }
    public string? Category { get; set; }
    public bool IsEnabled { get; set; }
    public string? AppliesTo { get; set; } // "all" or "specific"
}

