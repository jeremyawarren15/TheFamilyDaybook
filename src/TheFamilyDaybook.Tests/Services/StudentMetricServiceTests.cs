using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Tests.Helpers;
using TheFamilyDaybook.Web.Services;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Tests.Services;

[TestFixture]
public class StudentMetricServiceTests
{
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory = null!;
    private StudentMetricService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContextFactory = TestHelpers.CreateInMemoryDbContextFactory();
        _service = new StudentMetricService(_dbContextFactory);
    }

    [Test]
    public async Task GetMetricsForStudentAsync_ReturnsAllMetricsWithConfigs()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var templateMetric = TestHelpers.CreateTestMetric(id: 1, isTemplate: true, name: "Template Metric");
        var customMetric = TestHelpers.CreateTestMetric(id: 2, familyId: family.Id, isTemplate: false, name: "Custom Metric");
        context.Families.Add(family);
        context.Students.Add(student);
        context.Metrics.AddRange(templateMetric, customMetric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetMetricsForStudentAsync(student.Id, family.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var configs = result.ToList();
        Assert.That(configs.Count, Is.EqualTo(2));
        Assert.That(configs.All(c => c.MetricId == templateMetric.Id || c.MetricId == customMetric.Id), Is.True);
    }

    [Test]
    public async Task GetMetricsForStudentAsync_WithExistingConfig_ReturnsEnabledStatus()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var studentMetric = new StudentMetric
        {
            StudentId = student.Id,
            MetricId = metric.Id,
            IsEnabled = true,
            AppliesToAllSubjects = true,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentMetrics.Add(studentMetric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetMetricsForStudentAsync(student.Id, family.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var config = result.FirstOrDefault(c => c.MetricId == metric.Id);
        Assert.That(config, Is.Not.Null);
        Assert.That(config!.IsEnabled, Is.True);
        Assert.That(config.AppliesToAllSubjects, Is.True);
    }

    [Test]
    public async Task SaveStudentMetricConfigAsync_WithNewConfig_CreatesConfig()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var configs = new List<StudentMetricConfigModel>
        {
            new StudentMetricConfigModel
            {
                MetricId = metric.Id,
                IsEnabled = true,
                AppliesToAllSubjects = true
            }
        };

        // Act
        var result = await _service.SaveStudentMetricConfigAsync(student.Id, configs);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var created = await verifyContext.StudentMetrics
            .FirstOrDefaultAsync(sm => sm.StudentId == student.Id && sm.MetricId == metric.Id);
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.IsEnabled, Is.True);
    }

    [Test]
    public async Task SaveStudentMetricConfigAsync_WithDisabledConfig_RemovesConfig()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var studentMetric = new StudentMetric
        {
            StudentId = student.Id,
            MetricId = metric.Id,
            IsEnabled = true,
            AppliesToAllSubjects = true,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentMetrics.Add(studentMetric);
        await context.SaveChangesAsync();

        var configs = new List<StudentMetricConfigModel>
        {
            new StudentMetricConfigModel
            {
                MetricId = metric.Id,
                IsEnabled = false,
                AppliesToAllSubjects = true
            }
        };

        // Act
        var result = await _service.SaveStudentMetricConfigAsync(student.Id, configs);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var removed = await verifyContext.StudentMetrics
            .FirstOrDefaultAsync(sm => sm.StudentId == student.Id && sm.MetricId == metric.Id);
        Assert.That(removed, Is.Null);
    }

    [Test]
    public async Task UpdateStudentMetricAsync_WithEnabled_CreatesOrUpdates()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.UpdateStudentMetricAsync(student.Id, metric.Id, true, true);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var created = await verifyContext.StudentMetrics
            .FirstOrDefaultAsync(sm => sm.StudentId == student.Id && sm.MetricId == metric.Id);
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.IsEnabled, Is.True);
        Assert.That(created.AppliesToAllSubjects, Is.True);
    }

    [Test]
    public async Task UpdateStudentMetricAsync_WithDisabled_RemovesConfig()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var studentMetric = new StudentMetric
        {
            StudentId = student.Id,
            MetricId = metric.Id,
            IsEnabled = true,
            AppliesToAllSubjects = true,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentMetrics.Add(studentMetric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.UpdateStudentMetricAsync(student.Id, metric.Id, false, true);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var removed = await verifyContext.StudentMetrics
            .FirstOrDefaultAsync(sm => sm.StudentId == student.Id && sm.MetricId == metric.Id);
        Assert.That(removed, Is.Null);
    }

    [Test]
    public async Task EnableMetricForStudentAsync_CallsUpdateWithEnabled()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.EnableMetricForStudentAsync(student.Id, metric.Id, true);

        // Assert
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public async Task DisableMetricForStudentAsync_CallsUpdateWithDisabled()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var studentMetric = new StudentMetric
        {
            StudentId = student.Id,
            MetricId = metric.Id,
            IsEnabled = true,
            AppliesToAllSubjects = true,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentMetrics.Add(studentMetric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.DisableMetricForStudentAsync(student.Id, metric.Id);

        // Assert
        Assert.That(result.Succeeded, Is.True);
    }
}


