using Infrastructure.ExternalApis.Local;
using Infrastructure.Imaging;
using Microsoft.AspNetCore.Http;
using NetVips;

namespace Tests.Infrastructure.ExternalApis.Local;

public class LocalFileStorageConsumerTests
{
    [Fact]
    public async Task GivenUploadFileNameWithDirectory_WhenUploadFileAsync_ThenStoresFileInUploadsRoot()
    {
        var webRootPath = CreateTempDirectory();

        try
        {
            var sut = new LocalFileStorageConsumer(webRootPath);
            var file = new FormFile(new MemoryStream("content"u8.ToArray()), 0, 7, "file", "../avatar.png");

            var url = await sut.UploadFileAsync(file);

            url.ShouldContain("/uploads/");
            url.ShouldEndWith("-avatar.png");
            Directory.GetFiles(Path.Combine(webRootPath, "uploads")).Length.ShouldBe(1);
        }
        finally
        {
            Directory.Delete(webRootPath, recursive: true);
        }
    }

    [Fact]
    public async Task GivenUploadUrlWithSubDirectory_WhenDeleteFileWithUrl_ThenDeletesFileInSubDirectory()
    {
        var webRootPath = CreateTempDirectory();

        try
        {
            var filePath = Path.Combine(webRootPath, "uploads", "seed-partners", "file.png");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, "content");

            var sut = new LocalFileStorageConsumer(webRootPath);

            await sut.DeleteFileWithUrl("/uploads/seed-partners/file.png");

            File.Exists(filePath).ShouldBeFalse();
        }
        finally
        {
            Directory.Delete(webRootPath, recursive: true);
        }
    }

    [Fact]
    public async Task GivenUploadUrlOutsideUploads_WhenDeleteFileWithUrl_ThenDoesNotDeleteFile()
    {
        var webRootPath = CreateTempDirectory();

        try
        {
            var filePath = Path.Combine(webRootPath, "outside", "file.png");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            await File.WriteAllTextAsync(filePath, "content");

            var sut = new LocalFileStorageConsumer(webRootPath);

            await sut.DeleteFileWithUrl("/uploads/../outside/file.png");

            File.Exists(filePath).ShouldBeTrue();
        }
        finally
        {
            Directory.Delete(webRootPath, recursive: true);
        }
    }

    [Fact]
    public async Task GivenImageUpload_WhenUploadFile_ThenCreatesOptimizedVariants()
    {
        var tempDir = Directory.CreateTempSubdirectory("edb-storage-");
        try
        {
            var file = MakeImageFormFile("photo.jpg", "image/jpeg");
            var sut = new LocalFileStorageConsumer(tempDir.FullName, new VipsImageVariantGenerator());

            var url = await sut.UploadFileAsync(file);

            var filePath = Path.Combine(tempDir.FullName, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            File.Exists(filePath).ShouldBeTrue();
            File.Exists(filePath + ".avif").ShouldBeTrue();
            File.Exists(filePath + ".webp").ShouldBeTrue();
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task GivenImageUpload_WhenDeleteFile_ThenDeletesOptimizedVariants()
    {
        var tempDir = Directory.CreateTempSubdirectory("edb-storage-");
        try
        {
            var file = MakeImageFormFile("photo.jpg", "image/jpeg");
            var sut = new LocalFileStorageConsumer(tempDir.FullName, new VipsImageVariantGenerator());

            var url = await sut.UploadFileAsync(file);
            var filePath = Path.Combine(tempDir.FullName, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            await sut.DeleteFileWithUrl(url);

            File.Exists(filePath).ShouldBeFalse();
            File.Exists(filePath + ".avif").ShouldBeFalse();
            File.Exists(filePath + ".webp").ShouldBeFalse();
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"local-file-storage-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static IFormFile MakeImageFormFile(string fileName, string contentType)
    {
        using var image = Image.Black(32, 24).NewFromImage([255, 0, 0]);
        var bytes = image.WriteToBuffer(".jpg");
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
