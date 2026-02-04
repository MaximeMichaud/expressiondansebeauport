using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.FileStorage;

public interface IFileStorageApiConsumer
{
    Task DeleteFileWithUrl(string url);
    Task<string> UploadFileAsync(IFormFile file);
}