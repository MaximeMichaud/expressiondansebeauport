using Infrastructure.ExternalApis.Azure;
using Infrastructure.ExternalApis.Azure.Http;
using Infrastructure.ExternalApis.Azure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Tests.Infrastructure.ExternalApis.Azure.Client;

public class AzureApiHttpClientTests
{
    private const string ValidConnectionString = "DefaultEndpointsProtocol=https;AccountName=any;AccountKey=key;EndpointSuffix=core.windows.net";
    private const string AnyFileName = "file-name";
    private const string AnyFileContentType = "image/png";
    private readonly byte[] _fileStream = new byte[5];
    private readonly Uri _uri = new("https://example.com/550e8400-e29b-41d4-a716-446655440000");

    private readonly AzureApiHttpClient _azureApiHttpClient;
    private readonly Mock<IOptions<AzureApiSettings>> _azureSettings;
    private readonly Mock<IAzureBlobWrapper> _azureWrapper;

    public AzureApiHttpClientTests()
    {
        _azureSettings = new Mock<IOptions<AzureApiSettings>>();
        _azureWrapper = new Mock<IAzureBlobWrapper>();
        _azureApiHttpClient = new AzureApiHttpClient(_azureWrapper.Object);
    }

    [Fact]
    public async Task GivenRequestToAzureWasSuccessful_WhenUploadFileAsync_ThenFileAbsoluteUri()
    {
        // Arrange
        var settings = new AzureApiSettings { ConnectionString = ValidConnectionString };
        _azureSettings
            .Setup(x => x.Value)
            .Returns(settings);

        ConfigureAzureStorageAccount();

        // Act
        var absoluteUri = await _azureApiHttpClient.UploadFileAsync(
            AnyFileName,
            _fileStream,
            AnyFileContentType);

        // Assert
        absoluteUri.ShouldNotBeEmpty();
        absoluteUri.ShouldBe(_uri.AbsoluteUri);
    }

    private void ConfigureAzureStorageAccount()
    {
        var cloudBlockBlob = new Mock<CloudBlockBlob>(_uri);
        _azureWrapper
            .Setup(x => x.GetCloudBlockBlob(It.IsAny<string>()))
            .ReturnsAsync(cloudBlockBlob.Object);
    }
}