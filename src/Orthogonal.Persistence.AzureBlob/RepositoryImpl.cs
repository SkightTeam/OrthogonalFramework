using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Orthogonal.Persistence.AzureBlob
{
    public class RepositoryImpl<T> : Repository<T>
    {
        private BlobServiceClient blobServiceClient;
        private string blobContainerName;

        public RepositoryImpl(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient;
            blobContainerName = typeof(T).Name;
        }

        public async Task<T> get(string id)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlobClient(id);
            var download = await blobClient.DownloadAsync();
            return await JsonSerializer.DeserializeAsync<T>(download.Value.Content);
        }

        public IAsyncEnumerable<T> search(Query<T> query)
        {
            throw new NotImplementedException();
        }

        public  IAsyncEnumerable<T> search<TQuery>() where TQuery : Query<T>
        {
            if (typeof(TQuery).IsSubclassOf(typeof(FindAll<T>)))
            {
                var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
                var blobs = containerClient.GetBlobsAsync(BlobTraits.Metadata);
                return blobs.SelectAwait(async x => await get(x.Name));
            }
            else
            {
                throw new ArgumentException($" Query type {typeof(TQuery)} is not supported", "TQuery");
            }
        }

        public async Task save(T data)
        {
            var id = data.GetType().GetProperty("Id").GetValue(data).ToString();

            var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlobClient(id);
            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, data);
                stream.Position = 0;
                await blobClient.UploadAsync(stream);
            }
        }
    }
}
