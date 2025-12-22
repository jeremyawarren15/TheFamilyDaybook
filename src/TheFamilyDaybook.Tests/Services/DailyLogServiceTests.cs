using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Tests.Helpers;
using TheFamilyDaybook.Web.Services;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Tests.Services;

[TestFixture]
public class DailyLogServiceTests
{
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory = null!;
    private IStudentSubjectMetricService _studentSubjectMetricService = null!;
    private DailyLogService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContextFactory = TestHelpers.CreateInMemoryDbContextFactory();
        _studentSubjectMetricService = A.Fake<IStudentSubjectMetricService>();
        _service = new DailyLogService(_dbContextFactory, _studentSubjectMetricService);
    }

    [Test]
    public async Task GetDailyLogByIdAsync_WithValidId_ReturnsDailyLog()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var dailyLog = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.UtcNow.Date,
            Notes = "Test notes",
            CreatedAt = DateTime.UtcNow
        };
        context.DailyLogs.Add(dailyLog);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetDailyLogByIdAsync(dailyLog.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(dailyLog.Id));
        Assert.That(result.Notes, Is.EqualTo("Test notes"));
    }

    [Test]
    public async Task GetDailyLogByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetDailyLogByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetDailyLogAsync_WithValidParameters_ReturnsDailyLog()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var date = new DateTime(2024, 1, 15);
        var dailyLog = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = date.ToUniversalTime(),
            Notes = "Test notes",
            CreatedAt = DateTime.UtcNow
        };
        context.DailyLogs.Add(dailyLog);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetDailyLogAsync(student.Id, subject.Id, date);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Date.Date, Is.EqualTo(date.Date));
    }

    [Test]
    public async Task GetDailyLogAsync_ConvertsDateToUtc()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var localDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Local);
        var dailyLog = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = localDate.ToUniversalTime().Date,
            CreatedAt = DateTime.UtcNow
        };
        context.DailyLogs.Add(dailyLog);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetDailyLogAsync(student.Id, subject.Id, localDate);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task CreateDailyLogAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            Notes = "Test notes"
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public async Task CreateDailyLogAsync_WithInvalidStudent_ReturnsFailure()
    {
        // Arrange
        var model = new DailyLogModel
        {
            StudentId = 999,
            SubjectId = 1,
            Date = DateTime.Today
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Student not found"));
    }

    [Test]
    public async Task CreateDailyLogAsync_WithInvalidSubject_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = 999,
            Date = DateTime.Today
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Subject not found"));
    }

    [Test]
    public async Task CreateDailyLogAsync_WithDuplicateDate_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var date = DateTime.Today;
        var existingLog = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = date.ToUniversalTime().Date,
            CreatedAt = DateTime.UtcNow
        };
        context.DailyLogs.Add(existingLog);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = date
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("already exists"));
    }

    [Test]
    public async Task CreateDailyLogAsync_WithValidBooleanMetric_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, metricType: MetricType.Boolean);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            MetricValues = new List<MetricValueModel>
            {
                new MetricValueModel
                {
                    MetricId = metric.Id,
                    BooleanValue = true
                }
            }
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public async Task CreateDailyLogAsync_WithBooleanMetricMissingValue_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, name: "Test Boolean", metricType: MetricType.Boolean);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            MetricValues = new List<MetricValueModel>
            {
                new MetricValueModel
                {
                    MetricId = metric.Id,
                    BooleanValue = null
                }
            }
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Boolean value is required"));
    }

    [Test]
    public async Task CreateDailyLogAsync_WithBooleanMetricHavingOtherValues_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, name: "Test Boolean", metricType: MetricType.Boolean);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            MetricValues = new List<MetricValueModel>
            {
                new MetricValueModel
                {
                    MetricId = metric.Id,
                    BooleanValue = true,
                    CategoricalValue = "Invalid"
                }
            }
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Only boolean value should be set"));
    }

    [Test]
    public async Task CreateDailyLogAsync_WithValidCategoricalMetric_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var possibleValues = JsonSerializer.Serialize(new List<string> { "Option1", "Option2", "Option3" });
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, metricType: MetricType.Categorical);
        metric.PossibleValues = possibleValues;
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            MetricValues = new List<MetricValueModel>
            {
                new MetricValueModel
                {
                    MetricId = metric.Id,
                    CategoricalValue = "Option1"
                }
            }
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public async Task CreateDailyLogAsync_WithCategoricalMetricMissingValue_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, name: "Test Categorical", metricType: MetricType.Categorical);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            MetricValues = new List<MetricValueModel>
            {
                new MetricValueModel
                {
                    MetricId = metric.Id,
                    CategoricalValue = null
                }
            }
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Categorical value is required"));
    }

    [Test]
    public async Task CreateDailyLogAsync_WithCategoricalMetricInvalidValue_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var possibleValues = JsonSerializer.Serialize(new List<string> { "Option1", "Option2" });
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, name: "Test Categorical", metricType: MetricType.Categorical);
        metric.PossibleValues = possibleValues;
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            MetricValues = new List<MetricValueModel>
            {
                new MetricValueModel
                {
                    MetricId = metric.Id,
                    CategoricalValue = "InvalidOption"
                }
            }
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("is not valid"));
    }

    [Test]
    public async Task CreateDailyLogAsync_WithValidNumericMetric_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var numericConfig = JsonSerializer.Serialize(new Dictionary<string, object> { { "Min", 0 }, { "Max", 100 } });
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, metricType: MetricType.Numeric);
        metric.NumericConfig = numericConfig;
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            MetricValues = new List<MetricValueModel>
            {
                new MetricValueModel
                {
                    MetricId = metric.Id,
                    NumericValue = 50
                }
            }
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public async Task CreateDailyLogAsync_WithNumericMetricMissingValue_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, name: "Test Numeric", metricType: MetricType.Numeric);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            MetricValues = new List<MetricValueModel>
            {
                new MetricValueModel
                {
                    MetricId = metric.Id,
                    NumericValue = null
                }
            }
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Numeric value is required"));
    }

    [Test]
    public async Task CreateDailyLogAsync_WithNumericMetricBelowMin_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var numericConfig = JsonSerializer.Serialize(new Dictionary<string, object> { { "Min", 10 } });
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, name: "Test Numeric", metricType: MetricType.Numeric);
        metric.NumericConfig = numericConfig;
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            MetricValues = new List<MetricValueModel>
            {
                new MetricValueModel
                {
                    MetricId = metric.Id,
                    NumericValue = 5
                }
            }
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("must be at least"));
    }

    [Test]
    public async Task CreateDailyLogAsync_WithNumericMetricAboveMax_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var numericConfig = JsonSerializer.Serialize(new Dictionary<string, object> { { "Max", 100 } });
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id, name: "Test Numeric", metricType: MetricType.Numeric);
        metric.NumericConfig = numericConfig;
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        context.Metrics.Add(metric);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            MetricValues = new List<MetricValueModel>
            {
                new MetricValueModel
                {
                    MetricId = metric.Id,
                    NumericValue = 150
                }
            }
        };

        // Act
        var result = await _service.CreateDailyLogAsync(model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("must be at most"));
    }

    [Test]
    public async Task UpdateDailyLogAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var dailyLog = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today.ToUniversalTime().Date,
            Notes = "Original notes",
            CreatedAt = DateTime.UtcNow
        };
        context.DailyLogs.Add(dailyLog);
        await context.SaveChangesAsync();

        var model = new DailyLogModel
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today,
            Notes = "Updated notes"
        };

        // Act
        var result = await _service.UpdateDailyLogAsync(dailyLog.Id, model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public async Task UpdateDailyLogAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var model = new DailyLogModel
        {
            StudentId = 1,
            SubjectId = 1,
            Date = DateTime.Today
        };

        // Act
        var result = await _service.UpdateDailyLogAsync(999, model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Daily log not found"));
    }

    [Test]
    public async Task DeleteDailyLogAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var dailyLog = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today.ToUniversalTime().Date,
            CreatedAt = DateTime.UtcNow
        };
        context.DailyLogs.Add(dailyLog);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteDailyLogAsync(dailyLog.Id);

        // Assert
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public async Task DeleteDailyLogAsync_WithInvalidId_ReturnsFailure()
    {
        // Act
        var result = await _service.DeleteDailyLogAsync(999);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Daily log not found"));
    }

    [Test]
    public async Task GetDailyLogsForStudentAsync_ReturnsOrderedLogs()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var log1 = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today.AddDays(-2).ToUniversalTime().Date,
            CreatedAt = DateTime.UtcNow
        };
        var log2 = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.Today.ToUniversalTime().Date,
            CreatedAt = DateTime.UtcNow
        };
        context.DailyLogs.AddRange(log1, log2);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetDailyLogsForStudentAsync(student.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var logs = result.ToList();
        Assert.That(logs.Count, Is.EqualTo(2));
        Assert.That(logs[0].Date, Is.GreaterThanOrEqualTo(logs[1].Date)); // Ordered descending
    }
}


