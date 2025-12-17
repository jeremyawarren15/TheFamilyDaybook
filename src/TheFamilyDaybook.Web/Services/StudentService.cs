using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public class StudentService : IStudentService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public StudentService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<Student>> GetStudentsByFamilyIdAsync(int familyId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Students
            .Where(s => s.FamilyId == familyId)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Student?> GetStudentByIdAsync(int studentId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Students
            .FirstOrDefaultAsync(s => s.Id == studentId);
    }

    public async Task<StudentServiceResult> CreateStudentAsync(int familyId, StudentModel model)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            
            // Verify family exists
            var familyExists = await context.Families.AnyAsync(f => f.Id == familyId);
            if (!familyExists)
            {
                return StudentServiceResult.Failure("Family not found.");
            }

            var student = new Student
            {
                Name = model.Name,
                DateOfBirth = model.DateOfBirth.HasValue 
                    ? DateTime.SpecifyKind(model.DateOfBirth.Value, DateTimeKind.Utc) 
                    : null,
                Notes = model.Notes,
                FamilyId = familyId,
                CreatedAt = DateTime.UtcNow
            };

            context.Students.Add(student);
            await context.SaveChangesAsync();

            return StudentServiceResult.Success("Student created successfully!");
        }
        catch (Exception ex)
        {
            return StudentServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<StudentServiceResult> UpdateStudentAsync(int studentId, StudentModel model)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            
            var student = await context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null)
            {
                return StudentServiceResult.Failure("Student not found.");
            }

            student.Name = model.Name;
            student.DateOfBirth = model.DateOfBirth.HasValue 
                ? DateTime.SpecifyKind(model.DateOfBirth.Value, DateTimeKind.Utc) 
                : null;
            student.Notes = model.Notes;
            student.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return StudentServiceResult.Success("Student updated successfully!");
        }
        catch (Exception ex)
        {
            return StudentServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<StudentServiceResult> DeleteStudentAsync(int studentId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            
            var student = await context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null)
            {
                return StudentServiceResult.Failure("Student not found.");
            }

            context.Students.Remove(student);
            await context.SaveChangesAsync();

            return StudentServiceResult.Success("Student deleted successfully!");
        }
        catch (Exception ex)
        {
            return StudentServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }
}

