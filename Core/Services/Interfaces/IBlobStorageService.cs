using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Refundeo.Core.Models.Blob;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IBlobStorageService
    {
        Task UploadAsync(string containerName, string blobName, string filePath);
        Task UploadAsync(string containerName, string blobName, Stream stream);
        Task<MemoryStream> DownloadAsync(string containerName, string blobName);
        Task DownloadAsync(string containerName, string blobName, string path);
        Task DeleteAsync(string containerName, string blobName);
        Task<bool> ExistsAsync(string containerName, string blobName);
        Task<List<AzureBlobItem>> ListAsync(string containerName);
        Task<List<AzureBlobItem>> ListAsync(string containerName, string rootFolder);
        Task<List<string>> ListFoldersAsync(string containerName);
        Task<List<string>> ListFoldersAsync(string containerName, string rootFolder);
        Task<CloudBlockBlob> GetBlockBlobAsync(string containerName, string blobName);
    }
}
