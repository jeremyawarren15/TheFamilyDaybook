using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Tests.Helpers;
using TheFamilyDaybook.Web.Services;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Tests.Services;

[TestFixture]
public class StudentServiceTests
{
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory = null!;
    private StudentService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContextFactory = TestHelpers.CreateInMemoryDbContextFactory();
        _service = new StudentService(_dbContextFactory);
    }

    [Test]
    public async Task GetStudentsByFamilyIdAsync_ReturnsOrderedStudents()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student1 = TestHelpers.CreateTestStudent(id: 1, familyId: family.Id, name: "Zebra Student");
        var student2 = TestHelpers.CreateTestStudent(id: 2, familyId: family.Id, name: "Alpha Student");
        context.Families.Add(family);
        context.Students.AddRange(student1, student2);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetStudentsByFamilyIdAsync(family.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var students = result.ToList();
        Assert.That(students.Count, Is.EqualTo(2));
        Assert.That(students[0].Name, Is.EqualTo("Alpha Student")); // Ordered by name
        Assert.That(students[1].Name, Is.EqualTo("Zebra Student"));
    }

    [Test]
    public async Task GetStudentsByFamilyIdAsync_WithNoStudents_ReturnsEmpty()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        context.Families.Add(family);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetStudentsByFamilyIdAsync(family.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetStudentByIdAsync_WithValidId_ReturnsStudent()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id, name: "Test Student");
        context.Families.Add(family);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetStudentByIdAsync(student.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(student.Id));
        Assert.That(result.Name, Is.EqualTo("Test Student"));
    }

    [Test]
    public async Task GetStudentByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetStudentByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateStudentAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        context.Families.Add(family);
        await context.SaveChangesAsync();

        var model = new StudentModel
        {
            Name = "New Student",
            DateOfBirth = new DateTime(2010, 5, 15),
            Notes = "Test notes"
        };

        // Act
        var result = await _service.CreateStudentAsync(family.Id, model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);

        // Verify student was created
        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var created = await verifyContext.Students.FirstOrDefaultAsync(s => s.Name == "New Student");
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.DateOfBirth, Is.Not.Null);
        Assert.That(created.DateOfBirth!.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public async Task CreateStudentAsync_WithInvalidFamilyId_ReturnsFailure()
    {
        // Arrange
        var model = new StudentModel
        {
            Name = "New Student"
        };

        // Act
        var result = await _service.CreateStudentAsync(999, model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Family not found"));
    }

    [Test]
    public async Task CreateStudentAsync_ConvertsDateOfBirthToUtc()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        context.Families.Add(family);
        await context.SaveChangesAsync();

        var localDate = new DateTime(2010, 5, 15, 10, 30, 0, DateTimeKind.Local);
        var model = new StudentModel
        {
            Name = "New Student",
            DateOfBirth = localDate
        };

        // Act
        var result = await _service.CreateStudentAsync(family.Id, model);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var created = await verifyContext.Students.FirstOrDefaultAsync(s => s.Name == "New Student");
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.DateOfBirth!.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public async Task CreateStudentAsync_WithNullDateOfBirth_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        context.Families.Add(family);
        await context.SaveChangesAsync();

        var model = new StudentModel
        {
            Name = "New Student",
            DateOfBirth = null
        };

        // Act
        var result = await _service.CreateStudentAsync(family.Id, model);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var created = await verifyContext.Students.FirstOrDefaultAsync(s => s.Name == "New Student");
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.DateOfBirth, Is.Null);
    }

    [Test]
    public async Task UpdateStudentAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id, name: "Original Name");
        context.Families.Add(family);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var model = new StudentModel
        {
            Name = "Updated Name",
            DateOfBirth = new DateTime(2011, 6, 20),
            Notes = "Updated notes"
        };

        // Act
        var result = await _service.UpdateStudentAsync(student.Id, model);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var updated = await verifyContext.Students.FirstOrDefaultAsync(s => s.Id == student.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("Updated Name"));
        Assert.That(updated.UpdatedAt, Is.Not.Null);
    }

    [Test]
    public async Task UpdateStudentAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var model = new StudentModel
        {
            Name = "Updated Name"
        };

        // Act
        var result = await _service.UpdateStudentAsync(999, model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Student not found"));
    }

    [Test]
    public async Task UpdateStudentAsync_ConvertsDateOfBirthToUtc()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var localDate = new DateTime(2011, 6, 20, 10, 30, 0, DateTimeKind.Local);
        var model = new StudentModel
        {
            Name = "Updated Name",
            DateOfBirth = localDate
        };

        // Act
        var result = await _service.UpdateStudentAsync(student.Id, model);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var updated = await verifyContext.Students.FirstOrDefaultAsync(s => s.Id == student.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.DateOfBirth!.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public async Task DeleteStudentAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        context.Families.Add(family);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteStudentAsync(student.Id);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var deleted = await verifyContext.Students.FirstOrDefaultAsync(s => s.Id == student.Id);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task DeleteStudentAsync_WithInvalidId_ReturnsFailure()
    {
        // Act
        var result = await _service.DeleteStudentAsync(999);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Student not found"));
    }
}

