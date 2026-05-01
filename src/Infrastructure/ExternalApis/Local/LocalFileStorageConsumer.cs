using Application.Interfaces.FileStorage;
using Application.Interfaces.Imaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalApis.Local;

public class LocalFileStorageConsumer : IFileStorageApiConsumer
{
    private const string UploadFolder = "uploads";
    private readonly string _webRootPath;
    private readonly IImageVariantGenerator? _imageVariantGenerator;
    private readonly ILogger<LocalFileStorageConsumer>? _logger;

    public LocalFileStorageConsumer(
        string webRootPath,
        IImageVariantGenerator? imageVariantGenerator = null,
        ILogger<LocalFileStorageConsumer>? logger = null)
    {
        _webRootPath = webRootPath;
        _imageVariantGenerator = imageVariantGenerator;
        _logger = logger;

        var uploadPath = Path.Combine(_webRootPath, UploadFolder);
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "file";

        var uniqueFileName = $"{DateTime.Now.Ticks}-{fileName}";
        var filePath = Path.Combine(_webRootPath, UploadFolder, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        await TryGenerateImageVariants(filePath);

        return $"/{UploadFolder}/{uniqueFileName}";
    }

    public Task DeleteFileWithUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return Task.CompletedTask;

        var path = GetPathFromUrl(url);
        if (path is null || !path.StartsWith($"/{UploadFolder}/", StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        var relativePath = path[$"/{UploadFolder}/".Length..].Replace('/', Path.DirectorySeparatorChar);
        var uploadsPath = Path.GetFullPath(Path.Combine(_webRootPath, UploadFolder));
        var filePath = Path.GetFullPath(Path.Combine(uploadsPath, relativePath));

        if (!filePath.StartsWith(uploadsPath + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            return Task.CompletedTask;

        DeleteIfExists(filePath);
        DeleteIfExists(filePath + ".avif");
        DeleteIfExists(filePath + ".webp");

        return Task.CompletedTask;
    }

    private static string? GetPathFromUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
            return absoluteUri.LocalPath;

        if (Uri.TryCreate(url, UriKind.Relative, out var relativeUri))
            return relativeUri.OriginalString.Split('?', '#')[0];

        return null;
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

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await content.CopyToAsync(stream);
        }

        await TryGenerateImageVariants(filePath);

        var urlPath = string.IsNullOrWhiteSpace(subDirectory)
            ? $"/{UploadFolder}/{uniqueFileName}"
            : $"/{UploadFolder}/{subDirectory.Replace('\\', '/')}/{uniqueFileName}";

        return urlPath;
    }

    private async Task TryGenerateImageVariants(string filePath)
    {
        if (_imageVariantGenerator?.IsSupportedSourcePath(filePath) != true)
        {
            return;
        }

        try
        {
            await _imageVariantGenerator.EnsureVariantsAsync(filePath, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Unable to generate optimized image variants for {ImagePath}", filePath);
        }
    }

    private static void DeleteIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
