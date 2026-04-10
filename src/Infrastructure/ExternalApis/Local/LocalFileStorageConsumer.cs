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

    public async Task<string> UploadStreamAsync(Stream content, string fileName, string contentType, string? subDirectory = null)
    {
        fileName = Path.GetFileName(fileName);
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("fileName cannot be empty after sanitization.", nameof(fileName));

        if (!string.IsNullOrWhiteSpace(subDirectory) && (subDirectory.Contains("..") || subDirectory.Contains('/') || subDirectory.Contains('\\')))
            throw new ArgumentException("subDirectory cannot contain path separators or '..'", nameof(subDirectory));

        var folder = string.IsNullOrWhiteSpace(subDirectory)
            ? UploadFolder
            : Path.Combine(UploadFolder, subDirectory);

        var folderAbs = Path.Combine(_webRootPath, folder);
        if (!Directory.Exists(folderAbs))
            Directory.CreateDirectory(folderAbs);

        var uniqueFileName = $"{DateTime.Now.Ticks}-{fileName}";
        var filePath = Path.Combine(folderAbs, uniqueFileName);

        if (content.CanSeek) content.Position = 0;

        await using var stream = new FileStream(filePath, FileMode.Create);
        await content.CopyToAsync(stream);

        var urlPath = string.IsNullOrWhiteSpace(subDirectory)
            ? $"/{UploadFolder}/{uniqueFileName}"
            : $"/{UploadFolder}/{subDirectory.Replace('\\', '/')}/{uniqueFileName}";

        return urlPath;
    }
}
