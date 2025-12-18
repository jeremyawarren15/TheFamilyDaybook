namespace TheFamilyDaybook.Web.Services;

public class SubjectServiceResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public static SubjectServiceResult Success(string? message = null)
    {
        return new SubjectServiceResult
        {
            Succeeded = true,
            SuccessMessage = message
        };
    }

    public static SubjectServiceResult Failure(string errorMessage)
    {
        return new SubjectServiceResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}


