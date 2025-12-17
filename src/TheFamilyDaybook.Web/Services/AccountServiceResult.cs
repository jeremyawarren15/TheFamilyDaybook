namespace TheFamilyDaybook.Web.Services;

public class AccountServiceResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public static AccountServiceResult Success(string? message = null)
    {
        return new AccountServiceResult
        {
            Succeeded = true,
            SuccessMessage = message
        };
    }

    public static AccountServiceResult Failure(string errorMessage)
    {
        return new AccountServiceResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}

