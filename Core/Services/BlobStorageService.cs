﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Refundeo.Core.Helpers;
using Refundeo.Core.Models.Blob;
using Refundeo.Core.Services.Interfaces;

namespace Refundeo.Core.Services
{
    public class BlobStorageServiceService : IBlobStorageService
    {
        private readonly string _connectionString;

        public BlobStorageServiceService(IOptions<StorageAccountOptions> optionsAccessor)
        {
            _connectionString = optionsAccessor.Value.StorageAccountConnectionStringOption;
        }

        public async Task UploadAsync(string containerName, string blobName, string filePath)
        {
            var blockBlob = await GetBlockBlobAsync(containerName, blobName);

            using (var fileStream = File.Open(filePath, FileMode.Open))
            {
                fileStream.Position = 0;
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
        }

        public async Task UploadAsync(string containerName, string blobName, Stream stream)
        {
            var blockBlob = await GetBlockBlobAsync(containerName, blobName);

            stream.Position = 0;
            await blockBlob.UploadFromStreamAsync(stream);
        }

        public async Task<MemoryStream> DownloadAsync(string containerName, string blobName)
        {
            var blockBlob = await GetBlockBlobAsync(containerName, blobName);

            using (var stream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(stream);
                return stream;
            }
        }

        public async Task DownloadAsync(string containerName, string blobName, string path)
        {
            var blockBlob = await GetBlockBlobAsync(containerName, blobName);

            await blockBlob.DownloadToFileAsync(path, FileMode.Create);
        }

        public async Task DeleteAsync(string containerName, string blobName)
        {
            var blockBlob = await GetBlockBlobAsync(containerName, blobName);

            await blockBlob.DeleteAsync();
        }

        public async Task<bool> ExistsAsync(string containerName, string blobName)
        {
            var blockBlob = await GetBlockBlobAsync(containerName, blobName);

            return await blockBlob.ExistsAsync();
        }

        public async Task<List<AzureBlobItem>> ListAsync(string containerName)
        {
            return await GetBlobListAsync(containerName);
        }

        public async Task<List<AzureBlobItem>> ListAsync(string containerName, string rootFolder)
        {
            switch (rootFolder)
            {
                case "*":
                    return await ListAsync(containerName); //All Blobs
                case "/":
                    rootFolder = ""; //Root Blobs
                    break;
            }

            var list = await GetBlobListAsync(containerName);
            return list.Where(i => i.Folder == rootFolder).ToList();
        }

        public async Task<List<string>> ListFoldersAsync(string containerName)
        {
            var list = await GetBlobListAsync(containerName);
            return list.Where(i => !string.IsNullOrEmpty(i.Folder))
                .Select(i => i.Folder)
                .Distinct()
                .OrderBy(i => i)
                .ToList();
        }

        public async Task<List<string>> ListFoldersAsync(string containerName, string rootFolder)
        {
            if (rootFolder == "*" || rootFolder == "") return await ListFoldersAsync(containerName); //All Folders

            var list = await GetBlobListAsync(containerName);
            return list.Where(i => i.Folder.StartsWith(rootFolder))
                .Select(i => i.Folder)
                .Distinct()
                .OrderBy(i => i)
                .ToList();
        }

        public async Task<CloudBlockBlob> GetBlockBlobAsync(string containerName, string blobName)
        {
            var blobContainer = await GetContainerAsync(containerName);

            var blockBlob = blobContainer.GetBlockBlobReference(blobName);

            return blockBlob;
        }

        private async Task<CloudBlobContainer> GetContainerAsync(string containerName)
        {
            var storageAccount = new CloudStorageAccount(
                new StorageCredentials(_connectionString), false);

            var blobClient = storageAccount.CreateCloudBlobClient();

            var blobContainer = blobClient.GetContainerReference(containerName);
            await blobContainer.CreateIfNotExistsAsync();

            return blobContainer;
        }

        private async Task<List<AzureBlobItem>> GetBlobListAsync(string containerName, bool useFlatListing = true)
        {
            var blobContainer = await GetContainerAsync(containerName);

            var list = new List<AzureBlobItem>();
            BlobContinuationToken token = null;
            do
            {
                var resultSegment =
                    await blobContainer.ListBlobsSegmentedAsync("", useFlatListing, new BlobListingDetails(), null,
                        token, null, null);
                token = resultSegment.ContinuationToken;

                list.AddRange(resultSegment.Results.Select(item => new AzureBlobItem(item)));
            } while (token != null);

            return list.OrderBy(i => i.Folder).ThenBy(i => i.Name).ToList();
        }
    }
}
