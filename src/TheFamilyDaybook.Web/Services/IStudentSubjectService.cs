using TheFamilyDaybook.Models;

namespace TheFamilyDaybook.Web.Services;

public interface IStudentSubjectService
{
    Task<IEnumerable<Subject>> GetSubjectsForStudentAsync(int studentId);
    Task<IEnumerable<Student>> GetStudentsForSubjectAsync(int subjectId);
    Task<bool> StudentHasSubjectAsync(int studentId, int subjectId);
    Task<StudentServiceResult> AssignSubjectToStudentAsync(int studentId, int subjectId);
    Task<StudentServiceResult> RemoveSubjectFromStudentAsync(int studentId, int subjectId);
}

