using System.ComponentModel.DataAnnotations;

namespace TheFamilyDaybook.Models;

public class DailyLog
{
    public int Id { get; set; }
    
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    public int SubjectId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Student Student { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ICollection<DailyLogMetricValue> MetricValues { get; set; } = new List<DailyLogMetricValue>();
}

