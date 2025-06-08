using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace CareerWay.Shared.Storage.Azure;

public interface IAzureStorage : IStorage
{
    public BlobServiceClient BlobServiceClient { get; }

    public StorageSharedKeyCredential StorageSharedKeyCredential { get; }

    string GenerateSasUri(
        string containerName,
        DateTimeOffset expiresOn,
        string fileName,
        string resource = "b",
        BlobSasPermissions permissions = BlobSasPermissions.All
    );
}
