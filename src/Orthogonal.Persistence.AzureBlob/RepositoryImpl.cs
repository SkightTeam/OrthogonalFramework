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
        private BlobClientConfiguration blobClientConfiguration;
        private BlobServiceClient blobServiceClient;
        private string blobContainerName;

        public RepositoryImpl( BlobClientConfiguration blobClientConfiguration)
        {
            this.blobClientConfiguration = blobClientConfiguration;
            blobContainerName = typeof(T).Name.ToLower();
        }

        BlobServiceClient BlobServiceClient {
            get
            {
                return blobServiceClient ??
                       (blobServiceClient = new BlobServiceClient(blobClientConfiguration.ConnectionString));
            }
        }
        public async Task<T> get(string id)
        {
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
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
            if (typeof(TQuery).IsAssignableFrom(typeof(FindAll<T>)))
            {
                var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
                if (containerClient == null)
                {
                    containerClient = BlobServiceClient.CreateBlobContainer(blobContainerName);
                }
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
            var containerClient = BlobServiceClient.GetBlobContainerClient(blobContainerName);
            if (!containerClient.Exists().Value)
            {
                containerClient = BlobServiceClient.CreateBlobContainer(blobContainerName);
            }
            var blobClient = containerClient.GetBlobClient(id);
            using (var stream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(stream, data);
                stream.Position = 0;
                await blobClient.UploadAsync(stream);
            }
        }

        public async Task delete(string id)
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlobClient(id);
            await blobClient.DeleteAsync();
        }
    }
}
