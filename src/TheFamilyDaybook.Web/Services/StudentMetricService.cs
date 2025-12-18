using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public class StudentMetricService : IStudentMetricService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public StudentMetricService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<StudentMetricConfigModel>> GetMetricsForStudentAsync(int studentId, int familyId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        // Get all available metrics (templates + custom for family)
        var allMetrics = await context.Metrics
            .Where(m => m.IsTemplate || (m.FamilyId == familyId))
            .OrderBy(m => m.Category ?? "")
            .ThenBy(m => m.Name)
            .ToListAsync();

        // Get existing student metric configurations
        var studentMetrics = await context.StudentMetrics
            .Where(sm => sm.StudentId == studentId)
            .ToListAsync();

        // Build config models
        var configs = allMetrics.Select(metric =>
        {
            var existing = studentMetrics.FirstOrDefault(sm => sm.MetricId == metric.Id);
            return new StudentMetricConfigModel
            {
                MetricId = metric.Id,
                MetricName = metric.Name,
                MetricType = metric.MetricType,
                Category = metric.Category,
                IsEnabled = existing?.IsEnabled ?? false,
                AppliesToAllSubjects = existing?.AppliesToAllSubjects ?? true
            };
        }).ToList();

        return configs;
    }

    public async Task<StudentServiceResult> SaveStudentMetricConfigAsync(int studentId, List<StudentMetricConfigModel> configs)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            // Verify student exists
            var student = await context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null)
            {
                return StudentServiceResult.Failure("Student not found.");
            }

            // Get existing configurations
            var existingConfigs = await context.StudentMetrics
                .Where(sm => sm.StudentId == studentId)
                .ToListAsync();

            foreach (var config in configs)
            {
                var existing = existingConfigs.FirstOrDefault(sm => sm.MetricId == config.MetricId);

                if (config.IsEnabled)
                {
                    if (existing == null)
                    {
                        // Create new
                        var studentMetric = new StudentMetric
                        {
                            StudentId = studentId,
                            MetricId = config.MetricId,
                            IsEnabled = true,
                            AppliesToAllSubjects = config.AppliesToAllSubjects,
                            CreatedAt = DateTime.UtcNow
                        };
                        context.StudentMetrics.Add(studentMetric);
                    }
                    else
                    {
                        // Update existing
                        existing.IsEnabled = true;
                        existing.AppliesToAllSubjects = config.AppliesToAllSubjects;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    if (existing != null)
                    {
                        // Remove if exists
                        context.StudentMetrics.Remove(existing);
                    }
                }
            }

            await context.SaveChangesAsync();

            return StudentServiceResult.Success("Student metrics configured successfully!");
        }
        catch (Exception ex)
        {
            return StudentServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<StudentServiceResult> UpdateStudentMetricAsync(int studentId, int metricId, bool isEnabled, bool appliesToAllSubjects)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var existing = await context.StudentMetrics
                .FirstOrDefaultAsync(sm => sm.StudentId == studentId && sm.MetricId == metricId);

            if (isEnabled)
            {
                if (existing == null)
                {
                    var studentMetric = new StudentMetric
                    {
                        StudentId = studentId,
                        MetricId = metricId,
                        IsEnabled = true,
                        AppliesToAllSubjects = appliesToAllSubjects,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.StudentMetrics.Add(studentMetric);
                }
                else
                {
                    existing.IsEnabled = true;
                    existing.AppliesToAllSubjects = appliesToAllSubjects;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                if (existing != null)
                {
                    context.StudentMetrics.Remove(existing);
                }
            }

            await context.SaveChangesAsync();

            return StudentServiceResult.Success("Metric configuration updated successfully!");
        }
        catch (Exception ex)
        {
            return StudentServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }
}

