using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Data;
using TheFamilyDaybook.Models;

namespace TheFamilyDaybook.Web.Services;

public class StudentSubjectService : IStudentSubjectService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public StudentSubjectService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<Subject>> GetSubjectsForStudentAsync(int studentId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.StudentSubjects
            .Where(ss => ss.StudentId == studentId)
            .Include(ss => ss.Subject)
            .Select(ss => ss.Subject)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Student>> GetStudentsForSubjectAsync(int subjectId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.StudentSubjects
            .Where(ss => ss.SubjectId == subjectId)
            .Include(ss => ss.Student)
            .Select(ss => ss.Student)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<bool> StudentHasSubjectAsync(int studentId, int subjectId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.StudentSubjects
            .AnyAsync(ss => ss.StudentId == studentId && ss.SubjectId == subjectId);
    }

    public async Task<StudentServiceResult> AssignSubjectToStudentAsync(int studentId, int subjectId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            // Verify student and subject exist
            var student = await context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null)
            {
                return StudentServiceResult.Failure("Student not found.");
            }

            var subject = await context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
            if (subject == null)
            {
                return StudentServiceResult.Failure("Subject not found.");
            }

            // Check if already assigned
            var exists = await context.StudentSubjects
                .AnyAsync(ss => ss.StudentId == studentId && ss.SubjectId == subjectId);
            if (exists)
            {
                return StudentServiceResult.Failure("Subject is already assigned to this student.");
            }

            var studentSubject = new StudentSubject
            {
                StudentId = studentId,
                SubjectId = subjectId,
                CreatedAt = DateTime.UtcNow
            };

            context.StudentSubjects.Add(studentSubject);
            await context.SaveChangesAsync();

            return StudentServiceResult.Success("Subject assigned successfully!");
        }
        catch (Exception ex)
        {
            return StudentServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<StudentServiceResult> RemoveSubjectFromStudentAsync(int studentId, int subjectId)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var studentSubject = await context.StudentSubjects
                .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SubjectId == subjectId);
            if (studentSubject == null)
            {
                return StudentServiceResult.Failure("Subject assignment not found.");
            }

            context.StudentSubjects.Remove(studentSubject);
            await context.SaveChangesAsync();

            return StudentServiceResult.Success("Subject removed successfully!");
        }
        catch (Exception ex)
        {
            return StudentServiceResult.Failure($"An error occurred: {ex.Message}");
        }
    }
}

