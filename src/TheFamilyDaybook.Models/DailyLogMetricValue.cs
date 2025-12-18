using System.ComponentModel.DataAnnotations;

namespace TheFamilyDaybook.Models;

public class DailyLogMetricValue
{
    public int Id { get; set; }
    
    [Required]
    public int DailyLogId { get; set; }
    
    [Required]
    public int MetricId { get; set; }
    
    public bool? BooleanValue { get; set; }
    
    [MaxLength(200)]
    public string? CategoricalValue { get; set; }
    
    public decimal? NumericValue { get; set; }
    
    // Navigation properties
    public DailyLog DailyLog { get; set; } = null!;
    public Metric Metric { get; set; } = null!;
}

