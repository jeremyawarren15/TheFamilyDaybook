using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Contracts;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Tests.Helpers;

namespace TheFamilyDaybook.Tests.Data;

[TestFixture]
public class RepositoryTests
{
    private ApplicationDbContext _context = null!;
    private Repository<Student> _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _context = TestHelpers.CreateInMemoryDbContext();
        _repository = new Repository<Student>(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    [Test]
    public async Task GetByIdAsync_WithValidId_ReturnsEntity()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id, name: "Test Student");
        _context.Families.Add(family);
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(student.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(student.Id));
        Assert.That(result.Name, Is.EqualTo("Test Student"));
    }

    [Test]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student1 = TestHelpers.CreateTestStudent(id: 1, familyId: family.Id);
        var student2 = TestHelpers.CreateTestStudent(id: 2, familyId: family.Id);
        _context.Families.Add(family);
        _context.Students.AddRange(student1, student2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        var students = result.ToList();
        Assert.That(students.Count, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task FindAsync_WithPredicate_ReturnsMatchingEntities()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student1 = TestHelpers.CreateTestStudent(id: 1, familyId: family.Id, name: "John");
        var student2 = TestHelpers.CreateTestStudent(id: 2, familyId: family.Id, name: "Jane");
        _context.Families.Add(family);
        _context.Students.AddRange(student1, student2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(s => s.Name == "John");

        // Assert
        Assert.That(result, Is.Not.Null);
        var students = result.ToList();
        Assert.That(students.Count, Is.EqualTo(1));
        Assert.That(students[0].Name, Is.EqualTo("John"));
    }

    [Test]
    public async Task FirstOrDefaultAsync_WithPredicate_ReturnsFirstMatch()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id, name: "Test Student");
        _context.Families.Add(family);
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FirstOrDefaultAsync(s => s.Name == "Test Student");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Test Student"));
    }

    [Test]
    public async Task AddAsync_AddsEntity()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var student = TestHelpers.CreateTestStudent(familyId: family.Id, name: "New Student");

        // Act
        var result = await _repository.AddAsync(student);
        await _repository.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        var added = await _context.Students.FirstOrDefaultAsync(s => s.Name == "New Student");
        Assert.That(added, Is.Not.Null);
    }

    [Test]
    public async Task Update_UpdatesEntity()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id, name: "Original Name");
        _context.Families.Add(family);
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        // Act
        student.Name = "Updated Name";
        _repository.Update(student);
        await _repository.SaveChangesAsync();

        // Assert
        var updated = await _context.Students.FirstOrDefaultAsync(s => s.Id == student.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task Remove_RemovesEntity()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        _context.Families.Add(family);
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(student);
        await _repository.SaveChangesAsync();

        // Assert
        var removed = await _context.Students.FirstOrDefaultAsync(s => s.Id == student.Id);
        Assert.That(removed, Is.Null);
    }

    [Test]
    public async Task AnyAsync_WithMatchingEntities_ReturnsTrue()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student = TestHelpers.CreateTestStudent(familyId: family.Id);
        _context.Families.Add(family);
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.AnyAsync(s => s.Name == student.Name);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task AnyAsync_WithNoMatchingEntities_ReturnsFalse()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.AnyAsync(s => s.Name == "NonExistent");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CountAsync_WithPredicate_ReturnsCorrectCount()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student1 = TestHelpers.CreateTestStudent(id: 1, familyId: family.Id, name: "John");
        var student2 = TestHelpers.CreateTestStudent(id: 2, familyId: family.Id, name: "Jane");
        _context.Families.Add(family);
        _context.Students.AddRange(student1, student2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync(s => s.FamilyId == family.Id);

        // Assert
        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public async Task CountAsync_WithoutPredicate_ReturnsTotalCount()
    {
        // Arrange
        var family = TestHelpers.CreateTestFamily();
        var student1 = TestHelpers.CreateTestStudent(id: 1, familyId: family.Id);
        var student2 = TestHelpers.CreateTestStudent(id: 2, familyId: family.Id);
        _context.Families.Add(family);
        _context.Students.AddRange(student1, student2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync();

        // Assert
        Assert.That(result, Is.GreaterThanOrEqualTo(2));
    }
}


