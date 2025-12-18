using System.ComponentModel.DataAnnotations;

namespace TheFamilyDaybook.Models;

public class StudentSubjectMetric
{
    public int Id { get; set; }
    
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    public int SubjectId { get; set; }
    
    [Required]
    public int MetricId { get; set; }
    
    public bool IsEnabled { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Student Student { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Metric Metric { get; set; } = null!;
}

