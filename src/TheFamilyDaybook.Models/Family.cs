using System.ComponentModel.DataAnnotations;

namespace TheFamilyDaybook.Models;

public class Family
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}

