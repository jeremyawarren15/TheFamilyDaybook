using System.ComponentModel.DataAnnotations;

namespace TheFamilyDaybook.Models;

public class Student
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public DateTime? DateOfBirth { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign key to Family
    [Required]
    public int FamilyId { get; set; }
    
    // Navigation property
    public Family Family { get; set; } = null!;
}

