using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public class SubjectService : ISubjectService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public SubjectService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<Subject>> GetSubjectsByFamilyIdAsync(int familyId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Subjects
            .Where(s => s.FamilyId == familyId)
            .OrderBy(s => s.Name.ToLower())
            .ToListAsync();
    }

    public async Task<Subject?> GetSubjectByIdAsync(int subjectId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Subjects
            .FirstOrDefaultAsync(s => s.Id == subjectId);
    }

    public async Task<SubjectServiceResult> CreateSubjectAsync(int familyId, SubjectModel model)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            
            // Verify family exists
            var familyExists = await context.Families.AnyAsync(f => f.Id == familyId);
            if (!familyExists)
            {
                return SubjectServiceResult.Failure("Family not found.");
            }

            var subject = new Subject
            {
                Name = model.Name,
                Description = model.Description,
                FamilyId = familyId,
                CreatedAt = DateTime.UtcNow
            };

            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            return SubjectServiceResult.Success("Subject created successfully!");
        }
        catch (Exception ex)
        {
            return SubjectServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<SubjectServiceResult> UpdateSubjectAsync(int subjectId, SubjectModel model)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            
            var subject = await context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
            if (subject == null)
            {
                return SubjectServiceResult.Failure("Subject not found.");
            }

            subject.Name = model.Name;
            subject.Description = model.Description;
            subject.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return SubjectServiceResult.Success("Subject updated successfully!");
        }
        catch (Exception ex)
        {
            return SubjectServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<SubjectServiceResult> DeleteSubjectAsync(int subjectId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            
            var subject = await context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
            if (subject == null)
            {
                return SubjectServiceResult.Failure("Subject not found.");
            }

            context.Subjects.Remove(subject);
            await context.SaveChangesAsync();

            return SubjectServiceResult.Success("Subject deleted successfully!");
        }
        catch (Exception ex)
        {
            return SubjectServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }
}

