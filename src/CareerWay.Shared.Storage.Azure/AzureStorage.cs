using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CareerWay.Shared.Storage.Azure;

public class AzureStorage : IAzureStorage
{
    public BlobServiceClient BlobServiceClient { get; }

    public StorageSharedKeyCredential StorageSharedKeyCredential { get; }

    public AzureStorage(IOptions<AzureBlobStorageOptions> options)
    {
        BlobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
        StorageSharedKeyCredential = new StorageSharedKeyCredential(options.Value.AccountName, options.Value.AccountKey);
    }

    public List<string> GetFiles(string containerName)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        return blobContainerClient.GetBlobs().Select(b => b.Name).ToList();
    }

    public string GenerateSasUri(
        string containerName,
        DateTimeOffset expiresOn,
        string fileName,
        string resource = "b",
        BlobSasPermissions permissions = BlobSasPermissions.All)
    {
        var fileExtension = Path.GetExtension(fileName);
        var blobName = $"{Guid.NewGuid()}.{fileExtension}";
        var sasBuilder = new BlobSasBuilder(permissions, expiresOn)
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = resource
        };

        var sasToken = sasBuilder.ToSasQueryParameters(StorageSharedKeyCredential).ToString();
        UriBuilder fullUri = new UriBuilder()
        {
            Scheme = "https",
            Host = string.Format("{0}.blob.core.windows.net", StorageSharedKeyCredential.AccountName),
            Path = string.Format("{0}/{1}", containerName, blobName),
            Query = sasToken
        };
        return fullUri.ToString();
    }

    public async Task<string> UploadAsync(string containerName, IFormFile file)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        await blobContainerClient.CreateIfNotExistsAsync();
        await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);

        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{fileExtension}";

        BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);

        var header = new BlobHttpHeaders();
        header.ContentType = file.ContentType;

        return fileName;
    }

    public async Task DeleteAsync(string containerName, string fileName)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
        await blobClient.DeleteAsync();
    }

    public bool HasFile(string containerName, string fileName)
    {
        var blobContainerClient = BlobServiceClient.GetBlobContainerClient(containerName);
        var result = blobContainerClient.GetBlobs().Any(b => b.Name == fileName);
        return result;
    }
}
