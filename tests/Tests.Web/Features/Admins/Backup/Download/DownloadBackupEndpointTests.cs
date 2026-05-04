using Application.Interfaces.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Web.Features.Admins.Backup.Download;

namespace Tests.Web.Features.Admins.Backup.Download;

public class DownloadBackupEndpointTests
{
    private readonly Mock<IBackupService> _backupService = new();
    private readonly DefaultHttpContext _httpContext;
    private readonly DownloadBackupEndpoint _endpoint;

    public DownloadBackupEndpointTests()
    {
        _httpContext = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        _endpoint = Factory.Create<DownloadBackupEndpoint>(_httpContext, _backupService.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("../backup-2026-04-30-120000.tar.zst")]
    [InlineData("backup-2026-04-30-120000.zip")]
    public async Task GivenInvalidFileName_WhenHandleAsync_ThenReturnBadRequest(string fileName)
    {
        // Arrange
        var request = new DownloadBackupRequest { FileName = fileName };

        // Act
        await _endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        _backupService.Verify(
            x => x.GetFileStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GivenMissingBackupFile_WhenHandleAsync_ThenReturnNotFound()
    {
        // Arrange
        const string fileName = "backup-2026-04-30-120000.tar.zst";
        var request = new DownloadBackupRequest { FileName = fileName };

        _backupService
            .Setup(x => x.GetFileStreamAsync(fileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream?)null);

        // Act
        await _endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        _backupService.Verify(
            x => x.GetFileStreamAsync(fileName, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GivenBackupServiceRejectsFileName_WhenHandleAsync_ThenReturnBadRequest()
    {
        // Arrange
        const string fileName = "backup-2026-04-30-120000.tar.zst";
        var request = new DownloadBackupRequest { FileName = fileName };

        _backupService
            .Setup(x => x.GetFileStreamAsync(fileName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Nom de fichier invalide.", nameof(fileName)));

        // Act
        await _endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
    }
}
