using Infrastructure.ExternalApis.Azure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Infrastructure.ExternalApis.Azure;

public class AzureBlobWrapper : IAzureBlobWrapper
{
    private const string FOLDER_NAME = "images";

    private readonly IOptions<AzureApiSettings> _settings;

    public AzureBlobWrapper(IOptions<AzureApiSettings> settings)
    {
        _settings = settings;
    }
    
    public async Task<CloudBlockBlob> GetCloudBlockBlob(string fileName)
    {
        var cloudStorageAccount = CloudStorageAccount.Parse(_settings.Value.ConnectionString);
        var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        var cloudBlobContainer = await CreateCloudBlobContainer(cloudBlobClient, FOLDER_NAME);
        return cloudBlobContainer.GetBlockBlobReference(fileName);
    }

    private async Task<CloudBlobContainer> CreateCloudBlobContainer(CloudBlobClient cloudBlobClient, string containerName)
    {
        var cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);

        if (await cloudBlobContainer.CreateIfNotExistsAsync())
            await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

        return cloudBlobContainer;
    }
}
