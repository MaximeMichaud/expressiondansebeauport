namespace Infrastructure.ExternalApis.Azure.Http;

public interface IAzureApiHttpClient
{
    Task DeleteFileAsync(string fileName);
    Task<string> UploadFileAsync(string fileName, byte[] fileData, string fileMimeType);
}