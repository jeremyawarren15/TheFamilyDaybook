using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;

namespace TheFamilyDaybook.Tests.Helpers;

public static class TestHelpers
{
    public static IDbContextFactory<ApplicationDbContext> CreateInMemoryDbContextFactory(string? databaseName = null)
    {
        databaseName ??= Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var factory = new InMemoryDbContextFactory<ApplicationDbContext>(options);
        return factory;
    }

    public static ApplicationDbContext CreateInMemoryDbContext(string? databaseName = null)
    {
        databaseName ??= Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }

    public static async Task SeedDataAsync(IDbContextFactory<ApplicationDbContext> factory, params object[] entities)
    {
        using var context = await factory.CreateDbContextAsync();
        foreach (var entity in entities)
        {
            context.Add(entity);
        }
        await context.SaveChangesAsync();
    }

    public static Family CreateTestFamily(int id = 1, string name = "Test Family")
    {
        return new Family
        {
            Id = id,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Student CreateTestStudent(int id = 1, int familyId = 1, string name = "Test Student", DateTime? dateOfBirth = null)
    {
        return new Student
        {
            Id = id,
            FamilyId = familyId,
            Name = name,
            DateOfBirth = dateOfBirth?.ToUniversalTime(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Subject CreateTestSubject(int id = 1, int familyId = 1, string name = "Test Subject", string? description = null)
    {
        return new Subject
        {
            Id = id,
            FamilyId = familyId,
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Metric CreateTestMetric(int id = 1, int? familyId = null, string name = "Test Metric", MetricType metricType = MetricType.Boolean, bool isTemplate = false)
    {
        return new Metric
        {
            Id = id,
            FamilyId = familyId,
            Name = name,
            MetricType = metricType,
            IsTemplate = isTemplate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ApplicationUser CreateTestUser(string userId = "test-user-id", int? familyId = 1, string email = "test@example.com")
    {
        return new ApplicationUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            FamilyId = familyId,
            FirstName = "Test",
            LastName = "User"
        };
    }
}

// Simple implementation of IDbContextFactory for in-memory testing
internal class InMemoryDbContextFactory<TContext> : IDbContextFactory<TContext> where TContext : DbContext
{
    private readonly DbContextOptions<TContext> _options;

    public InMemoryDbContextFactory(DbContextOptions<TContext> options)
    {
        _options = options;
    }

    public TContext CreateDbContext()
    {
        return (TContext)Activator.CreateInstance(typeof(TContext), _options)!;
    }

    public Task<TContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateDbContext());
    }
}

