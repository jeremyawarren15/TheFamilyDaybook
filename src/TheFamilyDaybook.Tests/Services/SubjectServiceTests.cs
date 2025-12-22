using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Tests.Helpers;
using TheFamilyDaybook.Web.Services;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Tests.Services;

[TestFixture]
public class SubjectServiceTests
{
    private IDbContextFactory<ApplicationDbContext> _dbContextFactory = null!;
    private SubjectService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContextFactory = TestHelpers.CreateInMemoryDbContextFactory();
        _service = new SubjectService(_dbContextFactory);
    }

    [Test]
    public async Task GetSubjectsByFamilyIdAsync_ReturnsOrderedSubjects()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var subject1 = TestHelpers.CreateTestSubject(id: 1, familyId: family.Id, name: "Zebra Subject");
        var subject2 = TestHelpers.CreateTestSubject(id: 2, familyId: family.Id, name: "Alpha Subject");
        context.Families.Add(family);
        context.Subjects.AddRange(subject1, subject2);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetSubjectsByFamilyIdAsync(family.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        var subjects = result.ToList();
        Assert.That(subjects.Count, Is.EqualTo(2));
        Assert.That(subjects[0].Name.ToLower(), Is.LessThanOrEqualTo(subjects[1].Name.ToLower())); // Ordered by name (case-insensitive)
    }

    [Test]
    public async Task GetSubjectsByFamilyIdAsync_WithNoSubjects_ReturnsEmpty()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        context.Families.Add(family);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetSubjectsByFamilyIdAsync(family.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetSubjectByIdAsync_WithValidId_ReturnsSubject()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id, name: "Test Subject");
        context.Families.Add(family);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.GetSubjectByIdAsync(subject.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(subject.Id));
        Assert.That(result.Name, Is.EqualTo("Test Subject"));
    }

    [Test]
    public async Task GetSubjectByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetSubjectByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateSubjectAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        context.Families.Add(family);
        await context.SaveChangesAsync();

        var model = new SubjectModel
        {
            Name = "New Subject",
            Description = "Test description"
        };

        // Act
        var result = await _service.CreateSubjectAsync(family.Id, model);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);

        // Verify subject was created
        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var created = await verifyContext.Subjects.FirstOrDefaultAsync(s => s.Name == "New Subject");
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Description, Is.EqualTo("Test description"));
    }

    [Test]
    public async Task CreateSubjectAsync_WithInvalidFamilyId_ReturnsFailure()
    {
        // Arrange
        var model = new SubjectModel
        {
            Name = "New Subject"
        };

        // Act
        var result = await _service.CreateSubjectAsync(999, model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Family not found"));
    }

    [Test]
    public async Task UpdateSubjectAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id, name: "Original Name");
        context.Families.Add(family);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        var model = new SubjectModel
        {
            Name = "Updated Name",
            Description = "Updated description"
        };

        // Act
        var result = await _service.UpdateSubjectAsync(subject.Id, model);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var updated = await verifyContext.Subjects.FirstOrDefaultAsync(s => s.Id == subject.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("Updated Name"));
        Assert.That(updated.Description, Is.EqualTo("Updated description"));
        Assert.That(updated.UpdatedAt, Is.Not.Null);
    }

    [Test]
    public async Task UpdateSubjectAsync_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var model = new SubjectModel
        {
            Name = "Updated Name"
        };

        // Act
        var result = await _service.UpdateSubjectAsync(999, model);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Subject not found"));
    }

    [Test]
    public async Task DeleteSubjectAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var family = TestHelpers.CreateTestFamily();
        var subject = TestHelpers.CreateTestSubject(familyId: family.Id);
        context.Families.Add(family);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteSubjectAsync(subject.Id);

        // Assert
        Assert.That(result.Succeeded, Is.True);

        using var verifyContext = await _dbContextFactory.CreateDbContextAsync();
        var deleted = await verifyContext.Subjects.FirstOrDefaultAsync(s => s.Id == subject.Id);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task DeleteSubjectAsync_WithInvalidId_ReturnsFailure()
    {
        // Act
        var result = await _service.DeleteSubjectAsync(999);

        // Assert
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Subject not found"));
    }
}


