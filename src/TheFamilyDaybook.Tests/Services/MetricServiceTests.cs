using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Tests.Helpers;
using TheFamilyDaybook.Web.Services;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Tests.Services;

[TestFixture]
public class MetricServiceTests
{
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory = null!;
    private MetricService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContextFactory = TestHelpers.CreateInMemoryDbContextFactory();
        _service = new MetricService(_dbContextFactory);
    }

    [Test]
    public async Task GetAllMetricsAsync_ReturnsTemplatesAndFamilyMetrics()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var templateMetric = TestHelpers.CreateTestMetric(id: 1, isTemplate: true, name: "Template Metric");
        var customMetric = TestHelpers.CreateTestMetric(id: 2, familyId: family.Id, isTemplate: false, name: "Custom Metric");
        context.Families.Add(family);
        context.Metrics.AddRange(templateMetric, customMetric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllMetricsAsync(family.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var metrics = result.ToList();
        Assert.That(metrics.Count, Is.EqualTo(2));
        Assert.That(metrics[0].IsTemplate, Is.True); // Templates first
        Assert.That(metrics[1].IsTemplate, Is.False);
    }

    [Test]
    public async Task GetTemplateMetricsAsync_ReturnsOnlyTemplates()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var templateMetric = TestHelpers.CreateTestMetric(id: 1, isTemplate: true, name: "Template Metric");
        var customMetric = TestHelpers.CreateTestMetric(id: 2, familyId: family.Id, isTemplate: false, name: "Custom Metric");
        context.Families.Add(family);
        context.Metrics.AddRange(templateMetric, customMetric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetTemplateMetricsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        var metrics = result.ToList();
        Assert.That(metrics.Count, Is.EqualTo(1));
        Assert.That(metrics[0].IsTemplate, Is.True);
    }

    [Test]
    public async Task GetCustomMetricsByFamilyIdAsync_ReturnsOnlyCustomMetrics()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var templateMetric = TestHelpers.CreateTestMetric(id: 1, isTemplate: true, name: "Template Metric");
        var customMetric = TestHelpers.CreateTestMetric(id: 2, familyId: family.Id, isTemplate: false, name: "Custom Metric");
        context.Families.Add(family);
        context.Metrics.AddRange(templateMetric, customMetric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetCustomMetricsByFamilyIdAsync(family.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var metrics = result.ToList();
        Assert.That(metrics.Count, Is.EqualTo(1));
        Assert.That(metrics[0].IsTemplate, Is.False);
        Assert.That(metrics[0].FamilyId, Is.EqualTo(family.Id));
    }

    [Test]
    public async Task GetMetricByIdAsync_WithValidId_ReturnsMetric()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, name: "Test Metric");
        context.Families.Add(family);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetMetricByIdAsync(metric.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(metric.Id));
        Assert.That(result.Name, Is.EqualTo("Test Metric"));
    }

    [Test]
    public async Task GetMetricByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetMetricByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateMetricAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        context.Families.Add(family);
        await context.SaveChangesAsync();

        var model = new MetricModel
        {
            Name = "New Metric",
            Description = "Test description",
            MetricType = MetricType.Boolean,
            Category = "Test Category"
        };

        // Act
        var result = await _service.CreateMetricAsync(family.Id, model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);

        // Verify metric was created
        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var created = await verifyContext.Metrics.FirstOrDefaultAsync(m => m.Name == "New Metric");
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.IsTemplate, Is.False);
        Assert.That(created.FamilyId, Is.EqualTo(family.Id));
    }

    [Test]
    public async Task CreateMetricAsync_WithInvalidFamilyId_ReturnsFailure()
    {
        // Arrange
        var model = new MetricModel
        {
            Name = "New Metric",
            MetricType = MetricType.Boolean
        };

        // Act
        var result = await _service.CreateMetricAsync(999, model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Family not found"));
    }

    [Test]
    public async Task UpdateMetricAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, name: "Original Name", isTemplate: false);
        context.Families.Add(family);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var model = new MetricModel
        {
            Name = "Updated Name",
            Description = "Updated description",
            MetricType = MetricType.Categorical,
            Category = "Updated Category"
        };

        // Act
        var result = await _service.UpdateMetricAsync(metric.Id, model);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var updated = await verifyContext.Metrics.FirstOrDefaultAsync(m => m.Id == metric.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("Updated Name"));
        Assert.That(updated.UpdatedAt, Is.Not.Null);
    }

    [Test]
    public async Task UpdateMetricAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var model = new MetricModel
        {
            Name = "Updated Name",
            MetricType = MetricType.Boolean
        };

        // Act
        var result = await _service.UpdateMetricAsync(999, model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Metric not found"));
    }

    [Test]
    public async Task UpdateMetricAsync_WithTemplateMetric_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var templateMetric = TestHelpers.CreateTestMetric(id: 1, isTemplate: true, name: "Template Metric");
        context.Metrics.Add(templateMetric);
        await context.SaveChangesAsync();

        var model = new MetricModel
        {
            Name = "Updated Name",
            MetricType = MetricType.Boolean
        };

        // Act
        var result = await _service.UpdateMetricAsync(templateMetric.Id, model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Cannot update template metrics"));
    }

    [Test]
    public async Task DeleteMetricAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, isTemplate: false);
        context.Families.Add(family);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteMetricAsync(metric.Id);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var deleted = await verifyContext.Metrics.FirstOrDefaultAsync(m => m.Id == metric.Id);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task DeleteMetricAsync_WithInvalidId_ReturnsFailure()
    {
        // Act
        var result = await _service.DeleteMetricAsync(999);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Metric not found"));
    }

    [Test]
    public async Task DeleteMetricAsync_WithTemplateMetric_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var templateMetric = TestHelpers.CreateTestMetric(id: 1, isTemplate: true, name: "Template Metric");
        context.Metrics.Add(templateMetric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteMetricAsync(templateMetric.Id);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Cannot delete template metrics"));
    }
}


