namespace Domain.Common;

public class Error
{
    public string ErrorType { get; set; } = null!;
    public string ErrorMessage { get; set; } = null!;

    public Error() {}

    public Error(string errorType, string errorMessage)
    {
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }
}