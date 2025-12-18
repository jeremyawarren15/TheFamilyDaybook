namespace TheFamilyDaybook.Web.Services;

public class DailyLogServiceResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public static DailyLogServiceResult Success(string? message = null)
    {
        return new DailyLogServiceResult
        {
            Succeeded = true,
            SuccessMessage = message
        };
    }

    public static DailyLogServiceResult Failure(string errorMessage)
    {
        return new DailyLogServiceResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}

