using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Refundeo.Core.Models.Blob;

namespace Refundeo.Core.Services.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(string containerName, string blobName, string filePath);
        Task<string> UploadAsync(string containerName, string blobName, Stream stream);
        Task<string> UploadAsync(string containerName, string blobName, string base64, string imageType);
        Task<MemoryStream> DownloadAsync(string containerName, string blobName);
        Task DownloadAsync(string containerName, string blobName, string path);
        Task<MemoryStream> DownloadAsync(Uri uri);
        Task<byte[]> DownloadFromPathAsync(string path);
        Task DeleteAsync(string containerName, string blobName);
        Task DeleteAsync(Uri uri);
        Task<bool> ExistsAsync(Uri uri);
        Task<bool> ExistsAsync(string containerName, string blobName);
        Task<List<AzureBlobItem>> ListAsync(string containerName);
        Task<List<AzureBlobItem>> ListAsync(string containerName, string rootFolder);
        Task<List<string>> ListFoldersAsync(string containerName);
        Task<List<string>> ListFoldersAsync(string containerName, string rootFolder);
        Task<CloudBlockBlob> GetBlockBlobAsync(string containerName, string blobName);
        CloudBlockBlob GetBlockBlob(Uri uri);
    }
}
