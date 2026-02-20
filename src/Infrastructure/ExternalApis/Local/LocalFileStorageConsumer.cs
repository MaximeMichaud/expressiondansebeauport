using Application.Interfaces.FileStorage;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.ExternalApis.Local;

public class LocalFileStorageConsumer : IFileStorageApiConsumer
{
    private const string UploadFolder = "uploads";
    private readonly string _webRootPath;

    public LocalFileStorageConsumer(string webRootPath)
    {
        _webRootPath = webRootPath;

        var uploadPath = Path.Combine(_webRootPath, UploadFolder);
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var uniqueFileName = $"{DateTime.Now.Ticks}-{file.FileName}";
        var filePath = Path.Combine(_webRootPath, UploadFolder, uniqueFileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/{UploadFolder}/{uniqueFileName}";
    }

    public Task DeleteFileWithUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return Task.CompletedTask;

        string fileName;
        if (url.StartsWith('/'))
            fileName = Path.GetFileName(url);
        else
            fileName = Path.GetFileName(new Uri(url).LocalPath);

        var filePath = Path.Combine(_webRootPath, UploadFolder, fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }
}
