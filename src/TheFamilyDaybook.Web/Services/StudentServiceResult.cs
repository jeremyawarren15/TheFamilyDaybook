namespace TheFamilyDaybook.Web.Services;

public class StudentServiceResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public static StudentServiceResult Success(string? message = null)
    {
        return new StudentServiceResult
        {
            Succeeded = true,
            SuccessMessage = message
        };
    }

    public static StudentServiceResult Failure(string errorMessage)
    {
        return new StudentServiceResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}

