using Infrastructure.ExternalApis.Local;
using Microsoft.AspNetCore.Http;

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

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"local-file-storage-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
