using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Tests.Helpers;
using TheFamilyDaybook.Web.Services;

namespace TheFamilyDaybook.Tests.Services;

[TestFixture]
public class StudentSubjectServiceTests
{
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory = null!;
    private StudentSubjectService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContextFactory = TestHelpers.CreateInMemoryDbContextFactory();
        _service = new StudentSubjectService(_dbContextFactory);
    }

    [Test]
    public async Task GetSubjectsForStudentAsync_ReturnsOrderedSubjects()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        var subject1 = TestHelpers.CreateTestSubject(id: 1, familyId: family.Id, name: "Zebra Subject");
        var subject2 = TestHelpers.CreateTestSubject(id: 2, familyId: family.Id, name: "Alpha Subject");
        context.Families.Add(family);
        context.Students.Add(student);
        context.Subjects.AddRange(subject1, subject2);
        await context.SaveChangesAsync();

        var studentSubject1 = new StudentSubject
        {
            StudentId = student.Id,
            SubjectId = subject1.Id,
            CreatedAt = DateTime.UtcNow
        };
        var studentSubject2 = new StudentSubject
        {
            StudentId = student.Id,
            SubjectId = subject2.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentSubjects.AddRange(studentSubject1, studentSubject2);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetSubjectsForStudentAsync(student.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var subjects = result.ToList();
        Assert.That(subjects.Count, Is.EqualTo(2));
        Assert.That(subjects[0].Name, Is.EqualTo("Alpha Subject")); // Ordered by name
    }

    [Test]
    public async Task GetStudentsForSubjectAsync_ReturnsOrderedStudents()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student1 = TestHelpers.CreateTestStudent(id: 1, familyId: family.Id, name: "Zebra Student");
        var student2 = TestHelpers.CreateTestStudent(id: 2, familyId: family.Id, name: "Alpha Student");
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        context.Families.Add(family);
        context.Students.AddRange(student1, student2);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var studentSubject1 = new StudentSubject
        {
            StudentId = student1.Id,
            SubjectId = subject.Id,
            CreatedAt = DateTime.UtcNow
        };
        var studentSubject2 = new StudentSubject
        {
            StudentId = student2.Id,
            SubjectId = subject.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentSubjects.AddRange(studentSubject1, studentSubject2);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetStudentsForSubjectAsync(subject.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var students = result.ToList();
        Assert.That(students.Count, Is.EqualTo(2));
        Assert.That(students[0].Name, Is.EqualTo("Alpha Student")); // Ordered by name
    }

    [Test]
    public async Task StudentHasSubjectAsync_WithExistingAssignment_ReturnsTrue()
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

        var studentSubject = new StudentSubject
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentSubjects.Add(studentSubject);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.StudentHasSubjectAsync(student.Id, subject.Id);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task StudentHasSubjectAsync_WithNoAssignment_ReturnsFalse()
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

        // Act
        var result = await _service.StudentHasSubjectAsync(student.Id, subject.Id);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task AssignSubjectToStudentAsync_WithValidData_ReturnsSuccess()
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

        // Act
        var result = await _service.AssignSubjectToStudentAsync(student.Id, subject.Id);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);

        // Verify assignment was created
        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var assignment = await verifyContext.StudentSubjects
            .FirstOrDefaultAsync(ss => ss.StudentId == student.Id && ss.SubjectId == subject.Id);
        Assert.That(assignment, Is.Not.Null);
    }

    [Test]
    public async Task AssignSubjectToStudentAsync_WithInvalidStudentId_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        context.Families.Add(family);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.AssignSubjectToStudentAsync(999, subject.Id);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Student not found"));
    }

    [Test]
    public async Task AssignSubjectToStudentAsync_WithInvalidSubjectId_ReturnsFailure()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.AssignSubjectToStudentAsync(student.Id, 999);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Subject not found"));
    }

    [Test]
    public async Task AssignSubjectToStudentAsync_WithDuplicateAssignment_ReturnsFailure()
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

        var studentSubject = new StudentSubject
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentSubjects.Add(studentSubject);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.AssignSubjectToStudentAsync(student.Id, subject.Id);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("already assigned"));
    }

    [Test]
    public async Task RemoveSubjectFromStudentAsync_WithValidAssignment_ReturnsSuccess()
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

        var studentSubject = new StudentSubject
        {
            StudentId = student.Id,
            SubjectId = subject.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.StudentSubjects.Add(studentSubject);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.RemoveSubjectFromStudentAsync(student.Id, subject.Id);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        // Verify assignment was removed
        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var assignment = await verifyContext.StudentSubjects
            .FirstOrDefaultAsync(ss => ss.StudentId == student.Id && ss.SubjectId == subject.Id);
        Assert.That(assignment, Is.Null);
    }

    [Test]
    public async Task RemoveSubjectFromStudentAsync_WithNoAssignment_ReturnsFailure()
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

        // Act
        var result = await _service.RemoveSubjectFromStudentAsync(student.Id, subject.Id);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Subject assignment not found"));
    }
}

