using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public class MetricService : IMetricService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public MetricService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<Metric>> GetAllMetricsAsync(int familyId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Metrics
            .Where(m => m.IsTemplate || (m.FamilyId == familyId))
            .OrderBy(m => m.IsTemplate ? 0 : 1)
            .ThenBy(m => m.Category ?? "")
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Metric>> GetTemplateMetricsAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Metrics
            .Where(m => m.IsTemplate)
            .OrderBy(m => m.Category ?? "")
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Metric>> GetCustomMetricsByFamilyIdAsync(int familyId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Metrics
            .Where(m => !m.IsTemplate && m.FamilyId == familyId)
            .OrderBy(m => m.Category ?? "")
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<Metric?> GetMetricByIdAsync(int metricId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Metrics
            .FirstOrDefaultAsync(m => m.Id == metricId);
    }

    public async Task<MetricServiceResult> CreateMetricAsync(int familyId, MetricModel model)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            // Verify family exists
            var familyExists = await context.Families.AnyAsync(f => f.Id == familyId);
            if (!familyExists)
            {
                return MetricServiceResult.Failure("Family not found.");
            }

            var metric = new Metric
            {
                Name = model.Name,
                Description = model.Description,
                MetricType = model.MetricType,
                IsTemplate = false,
                FamilyId = familyId,
                Category = model.Category,
                PossibleValues = model.PossibleValues,
                NumericConfig = model.NumericConfig,
                CreatedAt = DateTime.UtcNow
            };

            context.Metrics.Add(metric);
            await context.SaveChangesAsync();

            return MetricServiceResult.Success("Metric created successfully!");
        }
        catch (Exception ex)
        {
            return MetricServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<MetricServiceResult> UpdateMetricAsync(int metricId, MetricModel model)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var metric = await context.Metrics.FirstOrDefaultAsync(m => m.Id == metricId);
            if (metric == null)
            {
                return MetricServiceResult.Failure("Metric not found.");
            }

            // Only allow updating custom metrics, not templates
            if (metric.IsTemplate)
            {
                return MetricServiceResult.Failure("Cannot update template metrics.");
            }

            metric.Name = model.Name;
            metric.Description = model.Description;
            metric.MetricType = model.MetricType;
            metric.Category = model.Category;
            metric.PossibleValues = model.PossibleValues;
            metric.NumericConfig = model.NumericConfig;
            metric.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return MetricServiceResult.Success("Metric updated successfully!");
        }
        catch (Exception ex)
        {
            return MetricServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<MetricServiceResult> DeleteMetricAsync(int metricId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var metric = await context.Metrics.FirstOrDefaultAsync(m => m.Id == metricId);
            if (metric == null)
            {
                return MetricServiceResult.Failure("Metric not found.");
            }

            // Only allow deleting custom metrics, not templates
            if (metric.IsTemplate)
            {
                return MetricServiceResult.Failure("Cannot delete template metrics.");
            }

            context.Metrics.Remove(metric);
            await context.SaveChangesAsync();

            return MetricServiceResult.Success("Metric deleted successfully!");
        }
        catch (Exception ex)
        {
            return MetricServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }
}

