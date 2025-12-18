using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public class DailyLogService : IDailyLogService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly IStudentSubjectMetricService _studentSubjectMetricService;

    public DailyLogService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        IStudentSubjectMetricService studentSubjectMetricService)
    {
        _dbContextFactory = dbContextFactory;
        _studentSubjectMetricService = studentSubjectMetricService;
    }

    public async Task<DailyLog?> GetDailyLogByIdAsync(int dailyLogId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.DailyLogs
            .Include(dl => dl.Student)
            .Include(dl => dl.Subject)
            .Include(dl => dl.MetricValues)
                .ThenInclude(mv => mv.Metric)
            .FirstOrDefaultAsync(dl => dl.Id == dailyLogId);
    }

    public async Task<DailyLog?> GetDailyLogAsync(int studentId, int subjectId, DateTime date)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        // Convert date to UTC for comparison
        var dateUtc = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        return await context.DailyLogs
            .Include(dl => dl.Student)
            .Include(dl => dl.Subject)
            .Include(dl => dl.MetricValues)
                .ThenInclude(mv => mv.Metric)
            .FirstOrDefaultAsync(dl => 
                dl.StudentId == studentId && 
                dl.SubjectId == subjectId && 
                dl.Date.Date == dateUtc.Date);
    }

    public async Task<IEnumerable<DailyLog>> GetDailyLogsForStudentAsync(int studentId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.DailyLogs
            .Where(dl => dl.StudentId == studentId)
            .Include(dl => dl.Student)
            .Include(dl => dl.Subject)
            .OrderByDescending(dl => dl.Date)
            .ThenBy(dl => dl.Subject.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyLog>> GetDailyLogsForStudentSubjectAsync(int studentId, int subjectId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.DailyLogs
            .Where(dl => dl.StudentId == studentId && dl.SubjectId == subjectId)
            .Include(dl => dl.Student)
            .Include(dl => dl.Subject)
            .OrderByDescending(dl => dl.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Metric>> GetAvailableMetricsForDailyLogAsync(int studentId, int subjectId, int familyId)
    {
        return await _studentSubjectMetricService.GetMetricsForDailyLogAsync(studentId, subjectId, familyId);
    }

    public async Task<DailyLogServiceResult> CreateDailyLogAsync(DailyLogModel model)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            // Verify student and subject exist
            var student = await context.Students.FirstOrDefaultAsync(s => s.Id == model.StudentId);
            if (student == null)
            {
                return DailyLogServiceResult.Failure("Student not found.");
            }

            var subject = await context.Subjects.FirstOrDefaultAsync(s => s.Id == model.SubjectId);
            if (subject == null)
            {
                return DailyLogServiceResult.Failure("Subject not found.");
            }

            // Check if log already exists for this student-subject-date
            // Convert date to UTC by creating a new DateTime from date components
            var dateUtc = new DateTime(model.Date.Year, model.Date.Month, model.Date.Day, 0, 0, 0, DateTimeKind.Utc);
            var existing = await context.DailyLogs
                .AnyAsync(dl => 
                    dl.StudentId == model.StudentId && 
                    dl.SubjectId == model.SubjectId && 
                    dl.Date.Date == dateUtc.Date);
            if (existing)
            {
                return DailyLogServiceResult.Failure("A daily log already exists for this student, subject, and date.");
            }

            // Create daily log
            var dailyLog = new DailyLog
            {
                StudentId = model.StudentId,
                SubjectId = model.SubjectId,
                Date = dateUtc,
                Notes = model.Notes,
                CreatedAt = DateTime.UtcNow
            };

            context.DailyLogs.Add(dailyLog);
            await context.SaveChangesAsync();

            // Add metric values
            foreach (var metricValue in model.MetricValues)
            {
                if (!IsMetricValueSet(metricValue))
                    continue;

                var metric = await context.Metrics.FirstOrDefaultAsync(m => m.Id == metricValue.MetricId);
                if (metric == null)
                    continue;

                // Validate metric value
                var validationResult = ValidateMetricValue(metricValue, metric);
                if (!validationResult.IsValid)
                {
                    return DailyLogServiceResult.Failure(validationResult.ErrorMessage!);
                }

                var dailyLogMetricValue = new DailyLogMetricValue
                {
                    DailyLogId = dailyLog.Id,
                    MetricId = metricValue.MetricId,
                    BooleanValue = metricValue.BooleanValue,
                    CategoricalValue = metricValue.CategoricalValue,
                    NumericValue = metricValue.NumericValue
                };

                context.DailyLogMetricValues.Add(dailyLogMetricValue);
            }

            await context.SaveChangesAsync();

            return DailyLogServiceResult.Success("Daily log created successfully!");
        }
        catch (Exception ex)
        {
            return DailyLogServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<DailyLogServiceResult> UpdateDailyLogAsync(int dailyLogId, DailyLogModel model)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var dailyLog = await context.DailyLogs
                .Include(dl => dl.MetricValues)
                .FirstOrDefaultAsync(dl => dl.Id == dailyLogId);
            if (dailyLog == null)
            {
                return DailyLogServiceResult.Failure("Daily log not found.");
            }

            // Check if date change would conflict with another log
            // Convert date to UTC by creating a new DateTime from date components
            var dateUtc = new DateTime(model.Date.Year, model.Date.Month, model.Date.Day, 0, 0, 0, DateTimeKind.Utc);
            if (dailyLog.Date.Date != dateUtc.Date)
            {
                var existing = await context.DailyLogs
                    .AnyAsync(dl => 
                        dl.Id != dailyLogId &&
                        dl.StudentId == model.StudentId && 
                        dl.SubjectId == model.SubjectId && 
                        dl.Date.Date == dateUtc.Date);
                if (existing)
                {
                    return DailyLogServiceResult.Failure("A daily log already exists for this student, subject, and date.");
                }
            }

            // Update daily log
            dailyLog.Date = dateUtc;
            dailyLog.Notes = model.Notes;
            dailyLog.UpdatedAt = DateTime.UtcNow;

            // Remove existing metric values
            context.DailyLogMetricValues.RemoveRange(dailyLog.MetricValues);

            // Add new metric values
            foreach (var metricValue in model.MetricValues)
            {
                if (!IsMetricValueSet(metricValue))
                    continue;

                var metric = await context.Metrics.FirstOrDefaultAsync(m => m.Id == metricValue.MetricId);
                if (metric == null)
                    continue;

                // Validate metric value
                var validationResult = ValidateMetricValue(metricValue, metric);
                if (!validationResult.IsValid)
                {
                    return DailyLogServiceResult.Failure(validationResult.ErrorMessage!);
                }

                var dailyLogMetricValue = new DailyLogMetricValue
                {
                    DailyLogId = dailyLog.Id,
                    MetricId = metricValue.MetricId,
                    BooleanValue = metricValue.BooleanValue,
                    CategoricalValue = metricValue.CategoricalValue,
                    NumericValue = metricValue.NumericValue
                };

                context.DailyLogMetricValues.Add(dailyLogMetricValue);
            }

            await context.SaveChangesAsync();

            return DailyLogServiceResult.Success("Daily log updated successfully!");
        }
        catch (Exception ex)
        {
            return DailyLogServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<DailyLogServiceResult> DeleteDailyLogAsync(int dailyLogId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var dailyLog = await context.DailyLogs
                .Include(dl => dl.MetricValues)
                .FirstOrDefaultAsync(dl => dl.Id == dailyLogId);
            if (dailyLog == null)
            {
                return DailyLogServiceResult.Failure("Daily log not found.");
            }

            context.DailyLogs.Remove(dailyLog);
            await context.SaveChangesAsync();

            return DailyLogServiceResult.Success("Daily log deleted successfully!");
        }
        catch (Exception ex)
        {
            return DailyLogServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    private bool IsMetricValueSet(MetricValueModel metricValue)
    {
        return metricValue.BooleanValue.HasValue ||
               !string.IsNullOrWhiteSpace(metricValue.CategoricalValue) ||
               metricValue.NumericValue.HasValue;
    }

    private (bool IsValid, string? ErrorMessage) ValidateMetricValue(MetricValueModel metricValue, Metric metric)
    {
        switch (metric.MetricType)
        {
            case MetricType.Boolean:
                if (!metricValue.BooleanValue.HasValue)
                    return (false, $"Boolean value is required for metric '{metric.Name}'.");
                if (!string.IsNullOrWhiteSpace(metricValue.CategoricalValue) || metricValue.NumericValue.HasValue)
                    return (false, $"Only boolean value should be set for metric '{metric.Name}'.");
                break;

            case MetricType.Categorical:
                if (string.IsNullOrWhiteSpace(metricValue.CategoricalValue))
                    return (false, $"Categorical value is required for metric '{metric.Name}'.");
                if (metricValue.BooleanValue.HasValue || metricValue.NumericValue.HasValue)
                    return (false, $"Only categorical value should be set for metric '{metric.Name}'.");
                
                // Validate against possible values
                if (!string.IsNullOrWhiteSpace(metric.PossibleValues))
                {
                    try
                    {
                        var possibleValues = JsonSerializer.Deserialize<List<string>>(metric.PossibleValues);
                        if (possibleValues != null && !possibleValues.Contains(metricValue.CategoricalValue))
                        {
                            return (false, $"Categorical value '{metricValue.CategoricalValue}' is not valid for metric '{metric.Name}'.");
                        }
                    }
                    catch
                    {
                        // If JSON parsing fails, skip validation
                    }
                }
                break;

            case MetricType.Numeric:
                if (!metricValue.NumericValue.HasValue)
                    return (false, $"Numeric value is required for metric '{metric.Name}'.");
                if (metricValue.BooleanValue.HasValue || !string.IsNullOrWhiteSpace(metricValue.CategoricalValue))
                    return (false, $"Only numeric value should be set for metric '{metric.Name}'.");
                
                // Validate against numeric config
                if (!string.IsNullOrWhiteSpace(metric.NumericConfig))
                {
                    try
                    {
                        var config = JsonSerializer.Deserialize<Dictionary<string, object>>(metric.NumericConfig);
                        if (config != null)
                        {
                            if (config.ContainsKey("Min") && decimal.TryParse(config["Min"].ToString(), out var min))
                            {
                                if (metricValue.NumericValue.Value < min)
                                    return (false, $"Numeric value must be at least {min} for metric '{metric.Name}'.");
                            }
                            if (config.ContainsKey("Max") && decimal.TryParse(config["Max"].ToString(), out var max))
                            {
                                if (metricValue.NumericValue.Value > max)
                                    return (false, $"Numeric value must be at most {max} for metric '{metric.Name}'.");
                            }
                        }
                    }
                    catch
                    {
                        // If JSON parsing fails, skip validation
                    }
                }
                break;
        }

        return (true, null);
    }
}

