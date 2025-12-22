using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Tests.Helpers;
using TheFamilyDaybook.Web.Services;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Tests.Services;

[TestFixture]
public class StudentSubjectMetricServiceTests
{
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory = null!;
    private StudentSubjectMetricService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContextFactory = TestHelpers.CreateInMemoryDbContextFactory();
        _service = new StudentSubjectMetricService(_dbContextFactory);
    }

    [Test]
    public async Task GetMetricsForDailyLogAsync_ReturnsEnabledMetrics()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric1 = TestHelpers.CreateTestMetric(id: 1, familyId: family.Id, name: "Metric 1");
        var metric2 = TestHelpers.CreateTestMetric(id: 2, familyId: family.Id, name: "Metric 2");
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.AddRange(metric1, metric2);
        await context.SaveChangesAsync();

        // Student-level metric that applies to all subjects
        var studentMetric = new StudentMetric
        {
            StudentId = student.Id,
            MetricId = metric1.Id,
            IsEnabled = true,
            AppliesToAllSubjects = true,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentMetrics.Add(studentMetric);

        // Subject-level metric
        var studentSubjectMetric = new StudentSubjectMetric
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            MetricId = metric2.Id,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentSubjectMetrics.Add(studentSubjectMetric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetMetricsForDailyLogAsync(student.Id, subject.Id, family.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var metrics = result.ToList();
        Assert.That(metrics.Count, Is.EqualTo(2));
        Assert.That(metrics.Any(m => m.Id == metric1.Id), Is.True);
        Assert.That(metrics.Any(m => m.Id == metric2.Id), Is.True);
    }

    [Test]
    public async Task GetMetricsForDailyLogAsync_ExcludesDisabledOverrides()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        // Student-level metric that applies to all subjects
        var studentMetric = new StudentMetric
        {
            StudentId = student.Id,
            MetricId = metric.Id,
            IsEnabled = true,
            AppliesToAllSubjects = true,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentMetrics.Add(studentMetric);

        // Subject-level override to disable
        var studentSubjectMetric = new StudentSubjectMetric
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            MetricId = metric.Id,
            IsEnabled = false,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentSubjectMetrics.Add(studentSubjectMetric);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetMetricsForDailyLogAsync(student.Id, subject.Id, family.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var metrics = result.ToList();
        Assert.That(metrics.Any(m => m.Id == metric.Id), Is.False); // Should be excluded
    }

    [Test]
    public async Task SaveStudentSubjectMetricConfigAsync_WithAppliesToAllSubjects_RemovesOverrideWhenEnabled()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        // Student-level metric that applies to all subjects
        var studentMetric = new StudentMetric
        {
            StudentId = student.Id,
            MetricId = metric.Id,
            IsEnabled = true,
            AppliesToAllSubjects = true,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentMetrics.Add(studentMetric);

        // Existing override
        var studentSubjectMetric = new StudentSubjectMetric
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            MetricId = metric.Id,
            IsEnabled = false,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentSubjectMetrics.Add(studentSubjectMetric);
        await context.SaveChangesAsync();

        var configs = new List<StudentSubjectMetricConfigModel>
        {
            new StudentSubjectMetricConfigModel
            {
                MetricId = metric.Id,
                IsEnabled = true,
                AppliesToAllSubjects = true
            }
        };

        // Act
        var result = await _service.SaveStudentSubjectMetricConfigAsync(student.Id, subject.Id, configs);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var overrideRemoved = await verifyContext.StudentSubjectMetrics
            .FirstOrDefaultAsync(ssm => ssm.StudentId == student.Id && ssm.SubjectId == subject.Id && ssm.MetricId == metric.Id);
        Assert.That(overrideRemoved, Is.Null); // Override should be removed
    }

    [Test]
    public async Task SaveStudentSubjectMetricConfigAsync_WithNotAppliesToAllSubjects_CreatesConfig()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        // Student-level metric that does NOT apply to all subjects
        var studentMetric = new StudentMetric
        {
            StudentId = student.Id,
            MetricId = metric.Id,
            IsEnabled = true,
            AppliesToAllSubjects = false,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentMetrics.Add(studentMetric);
        await context.SaveChangesAsync();

        var configs = new List<StudentSubjectMetricConfigModel>
        {
            new StudentSubjectMetricConfigModel
            {
                MetricId = metric.Id,
                IsEnabled = true,
                AppliesToAllSubjects = false
            }
        };

        // Act
        var result = await _service.SaveStudentSubjectMetricConfigAsync(student.Id, subject.Id, configs);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var created = await verifyContext.StudentSubjectMetrics
            .FirstOrDefaultAsync(ssm => ssm.StudentId == student.Id && ssm.SubjectId == subject.Id && ssm.MetricId == metric.Id);
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.IsEnabled, Is.True);
    }

    [Test]
    public async Task SaveStudentSubjectMetricConfigAsync_WithDisabled_CreatesOverride()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        // Student-level metric that applies to all subjects
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

        var configs = new List<StudentSubjectMetricConfigModel>
        {
            new StudentSubjectMetricConfigModel
            {
                MetricId = metric.Id,
                IsEnabled = false,
                AppliesToAllSubjects = true
            }
        };

        // Act
        var result = await _service.SaveStudentSubjectMetricConfigAsync(student.Id, subject.Id, configs);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var overrideCreated = await verifyContext.StudentSubjectMetrics
            .FirstOrDefaultAsync(ssm => ssm.StudentId == student.Id && ssm.SubjectId == subject.Id && ssm.MetricId == metric.Id);
        Assert.That(overrideCreated, Is.Not.Null);
        Assert.That(overrideCreated!.IsEnabled, Is.False);
    }
}


