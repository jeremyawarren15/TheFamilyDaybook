using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Tests.Helpers;

namespace TheFamilyDaybook.Tests.Data;

[TestFixture]
public class ApplicationDbContextTests
{
    private ApplicationDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _context = TestHelpers.CreateInMemoryDbContext();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    [Test]
    public async Task DeleteFamily_CascadesToStudents()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        _context.Families.Add(family);
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        // Act
        _context.Families.Remove(family);
        await _context.SaveChangesAsync();

        // Assert
        var deletedStudent = await _context.Students.FirstOrDefaultAsync(s => s.Id == student.Id);
        Assert.That(deletedStudent, Is.Null);
    }

    [Test]
    public async Task DeleteFamily_CascadesToSubjects()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        _context.Families.Add(family);
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        // Act
        _context.Families.Remove(family);
        await _context.SaveChangesAsync();

        // Assert
        var deletedSubject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == subject.Id);
        Assert.That(deletedSubject, Is.Null);
    }

    [Test]
    public async Task DeleteFamily_CascadesToMetrics()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        _context.Families.Add(family);
        _context.Metrics.Add(metric);
        await _context.SaveChangesAsync();

        // Act
        _context.Families.Remove(family);
        await _context.SaveChangesAsync();

        // Assert
        var deletedMetric = await _context.Metrics.FirstOrDefaultAsync(m => m.Id == metric.Id);
        Assert.That(deletedMetric, Is.Null);
    }

    [Test]
    public async Task DeleteStudent_CascadesToDailyLogs()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        _context.Families.Add(family);
        _context.Students.Add(student);
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        var dailyLog = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow
        };
        _context.DailyLogs.Add(dailyLog);
        await _context.SaveChangesAsync();

        // Act
        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        // Assert
        var deletedLog = await _context.DailyLogs.FirstOrDefaultAsync(dl => dl.Id == dailyLog.Id);
        Assert.That(deletedLog, Is.Null);
    }

    [Test]
    public async Task DeleteStudent_CascadesToStudentMetrics()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        _context.Families.Add(family);
        _context.Students.Add(student);
        _context.Metrics.Add(metric);
        await _context.SaveChangesAsync();

        var studentMetric = new StudentMetric
        {
            StudentId = student.Id,
            MetricId = metric.Id,
            IsEnabled = true,
            AppliesToAllSubjects = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.StudentMetrics.Add(studentMetric);
        await _context.SaveChangesAsync();

        // Act
        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        // Assert
        var deletedStudentMetric = await _context.StudentMetrics.FirstOrDefaultAsync(sm => sm.Id == studentMetric.Id);
        Assert.That(deletedStudentMetric, Is.Null);
    }

    [Test]
    public async Task DeleteSubject_CascadesToDailyLogs()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        _context.Families.Add(family);
        _context.Students.Add(student);
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        var dailyLog = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow
        };
        _context.DailyLogs.Add(dailyLog);
        await _context.SaveChangesAsync();

        // Act
        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();

        // Assert
        var deletedLog = await _context.DailyLogs.FirstOrDefaultAsync(dl => dl.Id == dailyLog.Id);
        Assert.That(deletedLog, Is.Null);
    }

    [Test]
    public async Task DeleteDailyLog_CascadesToMetricValues()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        _context.Families.Add(family);
        _context.Students.Add(student);
        _context.Subjects.Add(subject);
        _context.Metrics.Add(metric);
        await _context.SaveChangesAsync();

        var dailyLog = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow
        };
        _context.DailyLogs.Add(dailyLog);
        await _context.SaveChangesAsync();

        var metricValue = new DailyLogMetricValue
        {
            DailyLogId = dailyLog.Id,
            MetricId = metric.Id,
            BooleanValue = true
        };
        _context.DailyLogMetricValues.Add(metricValue);
        await _context.SaveChangesAsync();

        // Act
        _context.DailyLogs.Remove(dailyLog);
        await _context.SaveChangesAsync();

        // Assert
        var deletedMetricValue = await _context.DailyLogMetricValues.FirstOrDefaultAsync(mv => mv.Id == metricValue.Id);
        Assert.That(deletedMetricValue, Is.Null);
    }

    [Test]
    public async Task DeleteMetric_CascadesToDailyLogMetricValues()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        var metric = TestHelpers.CreateTestMetric(familyId: family.Id);
        _context.Families.Add(family);
        _context.Students.Add(student);
        _context.Subjects.Add(subject);
        _context.Metrics.Add(metric);
        await _context.SaveChangesAsync();

        var dailyLog = new DailyLog
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            Date = DateTime.UtcNow.Date,
            CreatedAt = DateTime.UtcNow
        };
        _context.DailyLogs.Add(dailyLog);
        await _context.SaveChangesAsync();

        var metricValue = new DailyLogMetricValue
        {
            DailyLogId = dailyLog.Id,
            MetricId = metric.Id,
            BooleanValue = true
        };
        _context.DailyLogMetricValues.Add(metricValue);
        await _context.SaveChangesAsync();

        // Act
        _context.Metrics.Remove(metric);
        await _context.SaveChangesAsync();

        // Assert
        var deletedMetricValue = await _context.DailyLogMetricValues.FirstOrDefaultAsync(mv => mv.Id == metricValue.Id);
        Assert.That(deletedMetricValue, Is.Null);
    }
}


