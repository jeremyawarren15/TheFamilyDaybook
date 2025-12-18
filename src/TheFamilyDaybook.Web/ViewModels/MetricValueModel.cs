using System.ComponentModel.DataAnnotations;
using TheFamilyDaybook.Models;

namespace TheFamilyDaybook.Web.ViewModels;

public class MetricValueModel
{
    public int MetricId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public MetricType MetricType { get; set; }
    public string? Category { get; set; }
    public bool? BooleanValue { get; set; }
    public string? CategoricalValue { get; set; }
    public decimal? NumericValue { get; set; }
    public string? PossibleValues { get; set; } // JSON string for categorical
    public string? NumericConfig { get; set; } // JSON string for numeric
}

