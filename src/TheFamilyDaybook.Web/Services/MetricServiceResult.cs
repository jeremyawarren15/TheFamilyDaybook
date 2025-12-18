namespace TheFamilyDaybook.Web.Services;

public class MetricServiceResult
{
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public static MetricServiceResult Success(string? message = null)
    {
        return new MetricServiceResult
        {
            Succeeded = true,
            SuccessMessage = message
        };
    }

    public static MetricServiceResult Failure(string errorMessage)
    {
        return new MetricServiceResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}

