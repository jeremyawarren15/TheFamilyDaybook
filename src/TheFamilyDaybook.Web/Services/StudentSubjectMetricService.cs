using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public class StudentSubjectMetricService : IStudentSubjectMetricService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public StudentSubjectMetricService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<StudentSubjectMetricConfigModel>> GetAvailableMetricsForStudentSubjectAsync(int studentId, int subjectId, int familyId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        // Get student-level metrics that don't apply to all subjects (need per-subject config)
        var perSubjectMetrics = await context.StudentMetrics
            .Where(sm => sm.StudentId == studentId && sm.IsEnabled && !sm.AppliesToAllSubjects)
            .Include(sm => sm.Metric)
            .Select(sm => sm.Metric)
            .ToListAsync();

        // Get student-level metrics that apply to all subjects (can be overridden)
        var allSubjectsMetrics = await context.StudentMetrics
            .Where(sm => sm.StudentId == studentId && sm.IsEnabled && sm.AppliesToAllSubjects)
            .Include(sm => sm.Metric)
            .Select(sm => sm.Metric)
            .ToListAsync();

        // Get all available metrics (templates + custom for family)
        var allAvailableMetrics = await context.Metrics
            .Where(m => m.IsTemplate || (m.FamilyId == familyId))
            .ToListAsync();

        // Combine: metrics that need per-subject config + metrics that can be overridden
        var metricsToShow = perSubjectMetrics
            .Concat(allSubjectsMetrics)
            .GroupBy(m => m.Id)
            .Select(g => g.First())
            .ToList();

        // Get existing student-subject metric configurations
        var studentSubjectMetrics = await context.StudentSubjectMetrics
            .Where(ssm => ssm.StudentId == studentId && ssm.SubjectId == subjectId)
            .ToListAsync();

        // Get student-level metrics for reference
        var studentMetrics = await context.StudentMetrics
            .Where(sm => sm.StudentId == studentId)
            .ToListAsync();

        // Build config models
        var configs = metricsToShow.Select(metric =>
        {
            var existing = studentSubjectMetrics.FirstOrDefault(ssm => ssm.MetricId == metric.Id);
            var studentMetric = studentMetrics.FirstOrDefault(sm => sm.MetricId == metric.Id);
            var appliesToAll = studentMetric?.AppliesToAllSubjects ?? false;
            
            // If metric applies to all subjects and no override exists, it's enabled by default
            // If override exists, use the override value
            // If metric doesn't apply to all subjects, use override value or default to false
            var isEnabled = existing != null 
                ? existing.IsEnabled 
                : (appliesToAll ? true : false);

            return new StudentSubjectMetricConfigModel
            {
                MetricId = metric.Id,
                MetricName = metric.Name,
                MetricType = metric.MetricType,
                Category = metric.Category,
                IsEnabled = isEnabled,
                AppliesToAllSubjects = appliesToAll // Add this to the model to show in UI
            };
        }).OrderBy(c => c.Category ?? "")
          .ThenBy(c => c.MetricName)
          .ToList();

        return configs;
    }

    public async Task<IEnumerable<Metric>> GetMetricsForDailyLogAsync(int studentId, int subjectId, int familyId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        // Get metrics where StudentMetric has IsEnabled = true and AppliesToAllSubjects = true
        var studentLevelMetrics = await context.StudentMetrics
            .Where(sm => sm.StudentId == studentId && sm.IsEnabled && sm.AppliesToAllSubjects)
            .Include(sm => sm.Metric)
            .Select(sm => sm.Metric)
            .ToListAsync();

        // Get metrics where StudentSubjectMetric has IsEnabled = true
        var subjectLevelMetrics = await context.StudentSubjectMetrics
            .Where(ssm => ssm.StudentId == studentId && ssm.SubjectId == subjectId && ssm.IsEnabled)
            .Include(ssm => ssm.Metric)
            .Select(ssm => ssm.Metric)
            .ToListAsync();

        // Get metrics where StudentSubjectMetric has IsEnabled = false (to exclude)
        var excludedMetrics = await context.StudentSubjectMetrics
            .Where(ssm => ssm.StudentId == studentId && ssm.SubjectId == subjectId && !ssm.IsEnabled)
            .Select(ssm => ssm.MetricId)
            .ToListAsync();

        // Combine student-level and subject-level metrics, excluding overrides
        var allMetrics = studentLevelMetrics
            .Concat(subjectLevelMetrics)
            .Where(m => !excludedMetrics.Contains(m.Id))
            .GroupBy(m => m.Id)
            .Select(g => g.First())
            .OrderBy(m => m.Category ?? "")
            .ThenBy(m => m.Name)
            .ToList();

        return allMetrics;
    }

    public async Task<StudentServiceResult> SaveStudentSubjectMetricConfigAsync(int studentId, int subjectId, List<StudentSubjectMetricConfigModel> configs)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            // Verify student and subject exist
            var student = await context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null)
            {
                return StudentServiceResult.Failure("Student not found.");
            }

            var subject = await context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
            if (subject == null)
            {
                return StudentServiceResult.Failure("Subject not found.");
            }

            // Get existing configurations
            var existingConfigs = await context.StudentSubjectMetrics
                .Where(ssm => ssm.StudentId == studentId && ssm.SubjectId == subjectId)
                .ToListAsync();

            // Get student-level metrics to check which ones apply to all subjects
            var studentMetrics = await context.StudentMetrics
                .Where(sm => sm.StudentId == studentId)
                .ToListAsync();

            foreach (var config in configs)
            {
                var existing = existingConfigs.FirstOrDefault(ssm => ssm.MetricId == config.MetricId);
                var studentMetric = studentMetrics.FirstOrDefault(sm => sm.MetricId == config.MetricId);
                var appliesToAll = studentMetric?.AppliesToAllSubjects ?? false;

                if (config.IsEnabled)
                {
                    // If metric applies to all subjects and user enables it, remove any override
                    // (let the student-level setting take effect)
                    if (appliesToAll)
                    {
                        if (existing != null)
                        {
                            // Remove override to let student-level setting apply
                            context.StudentSubjectMetrics.Remove(existing);
                        }
                        // Don't create an entry if it applies to all subjects and is enabled
                    }
                    else
                    {
                        // Metric doesn't apply to all subjects, so we need per-subject config
                        if (existing == null)
                        {
                            // Create new
                            var studentSubjectMetric = new StudentSubjectMetric
                            {
                                StudentId = studentId,
                                SubjectId = subjectId,
                                MetricId = config.MetricId,
                                IsEnabled = true,
                                CreatedAt = DateTime.UtcNow
                            };
                            context.StudentSubjectMetrics.Add(studentSubjectMetric);
                        }
                        else
                        {
                            // Update existing
                            existing.IsEnabled = true;
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }
                else
                {
                    // User wants to disable the metric for this subject
                    if (existing == null)
                    {
                        // Create override to disable (even if it applies to all subjects)
                        var studentSubjectMetric = new StudentSubjectMetric
                        {
                            StudentId = studentId,
                            SubjectId = subjectId,
                            MetricId = config.MetricId,
                            IsEnabled = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        context.StudentSubjectMetrics.Add(studentSubjectMetric);
                    }
                    else
                    {
                        // Update existing override
                        existing.IsEnabled = false;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await context.SaveChangesAsync();

            return StudentServiceResult.Success("Student-subject metrics configured successfully!");
        }
        catch (Exception ex)
        {
            return StudentServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }
}

