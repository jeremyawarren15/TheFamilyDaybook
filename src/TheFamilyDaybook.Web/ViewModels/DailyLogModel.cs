using System.ComponentModel.DataAnnotations;

namespace TheFamilyDaybook.Web.ViewModels;

public class DailyLogModel
{
    public int? Id { get; set; }

    [Required]
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    [Required]
    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; } = DateTime.Today;

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }

    public List<MetricValueModel> MetricValues { get; set; } = new();
}

