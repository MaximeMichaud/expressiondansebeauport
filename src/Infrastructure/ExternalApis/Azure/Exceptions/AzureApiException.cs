namespace Infrastructure.ExternalApis.Azure.Exceptions;

public class AzureApiException : Exception
{
    public AzureApiException(string message) : base(message) { }
}