using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TheFamilyDaybook.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key to Family
    public int? FamilyId { get; set; }
    
    // Navigation property
    public Family? Family { get; set; }
}

