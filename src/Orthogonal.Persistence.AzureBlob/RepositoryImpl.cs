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
        private readonly BlobClientConfiguration blobClientConfiguration;
        private BlobServiceClient blobServiceClient;
        private readonly Func<Type, string> default_name_convention = t => t.Name.ToLower();
        private string blob_name;

        public RepositoryImpl(BlobClientConfiguration blobClientConfiguration)
        {
            this.blobClientConfiguration = blobClientConfiguration;
            var name_convention = blobClientConfiguration.NameConvention ?? default_name_convention;
            blob_name = name_convention(typeof(T)) ;
            if (string.IsNullOrEmpty(blob_name))
            {
                blob_name = default_name_convention(typeof(T));
            }
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
            var containerClient = BlobServiceClient.GetBlobContainerClient(blob_name);
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
                var containerClient = BlobServiceClient.GetBlobContainerClient(blob_name);
                if (containerClient == null)
                {
                    containerClient = BlobServiceClient.CreateBlobContainer(blob_name);
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
            var containerClient = BlobServiceClient.GetBlobContainerClient(blob_name);
            if (!containerClient.Exists().Value)
            {
                containerClient = BlobServiceClient.CreateBlobContainer(blob_name);
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
            var containerClient = blobServiceClient.GetBlobContainerClient(blob_name);
            var blobClient = containerClient.GetBlobClient(id);
            await blobClient.DeleteAsync();
        }
    }
}
