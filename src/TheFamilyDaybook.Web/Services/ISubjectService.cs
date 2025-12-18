using TheFamilyDaybook.Models;
using TheFamilyDaybook.Web.ViewModels;

namespace TheFamilyDaybook.Web.Services;

public interface ISubjectService
{
    Task<IEnumerable<Subject>> GetSubjectsByFamilyIdAsync(int familyId);
    Task<Subject?> GetSubjectByIdAsync(int subjectId);
    Task<SubjectServiceResult> CreateSubjectAsync(int familyId, SubjectModel model);
    Task<SubjectServiceResult> UpdateSubjectAsync(int subjectId, SubjectModel model);
    Task<SubjectServiceResult> DeleteSubjectAsync(int subjectId);
}


