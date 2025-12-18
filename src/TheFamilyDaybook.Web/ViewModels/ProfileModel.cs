using System.ComponentModel.DataAnnotations;

namespace TheFamilyDaybook.Web.ViewModels;

public class ProfileModel
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
}


