using System.ComponentModel.DataAnnotations;
using TheFamilyDaybook.Models;

namespace TheFamilyDaybook.Web.ViewModels;

public class MetricModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Metric type is required")]
    public MetricType MetricType { get; set; }

    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    [StringLength(2000, ErrorMessage = "Possible values cannot exceed 2000 characters")]
    public string? PossibleValues { get; set; } // JSON string for categorical metrics

    [StringLength(500, ErrorMessage = "Numeric config cannot exceed 500 characters")]
    public string? NumericConfig { get; set; } // JSON string for numeric metrics
}

