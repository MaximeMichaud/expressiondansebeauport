namespace Domain.Common;

public class Error
{
    public string ErrorType { get; set; } = default!;
    public string ErrorMessage { get; set; } = default!;

    public Error() {}

    public Error(string errorType, string errorMessage)
    {
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }
}