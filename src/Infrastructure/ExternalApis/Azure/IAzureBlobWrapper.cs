using Microsoft.WindowsAzure.Storage.Blob;

namespace Infrastructure.ExternalApis.Azure;

public interface IAzureBlobWrapper
{
    Task<CloudBlockBlob> GetCloudBlockBlob(string fileName);
}
