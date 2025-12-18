using System.ComponentModel.DataAnnotations;

namespace TheFamilyDaybook.Models;

public class Metric
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Required]
    public MetricType MetricType { get; set; }
    
    public bool IsTemplate { get; set; } = false;
    
    public int? FamilyId { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [MaxLength(2000)]
    public string? PossibleValues { get; set; } // JSON string for categorical metrics
    
    [MaxLength(500)]
    public string? NumericConfig { get; set; } // JSON string for numeric metrics
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Family? Family { get; set; }
}

