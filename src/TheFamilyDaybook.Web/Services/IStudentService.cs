using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public interface IStudentService
{
    Task<IEnumerable<Student>> GetStudentsByFamilyIdAsync(int familyId);
    Task<Student?> GetStudentByIdAsync(int studentId);
    Task<StudentServiceResult> CreateStudentAsync(int familyId, StudentModel model);
    Task<StudentServiceResult> UpdateStudentAsync(int studentId, StudentModel model);
    Task<StudentServiceResult> DeleteStudentAsync(int studentId);
}

