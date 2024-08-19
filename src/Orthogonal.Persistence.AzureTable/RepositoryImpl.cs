using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace Orthogonal.Persistence.AzureTable
{
    public class RepositoryImpl<T> : Repository<T> where T : ITableEntity
    {
        private readonly AzureTableConfiguration configuration;
        private TableServiceClient? tableServiceClient;
        private TableClient? tableClient;

        public RepositoryImpl(AzureTableConfiguration configuration)
        {
            this.configuration = configuration;
        }

        TableServiceClient TableServiceClient => 
            tableServiceClient ??= new TableServiceClient(connectionString:configuration.ConnectionString);

        private TableClient TableClient =>
            tableClient ??= TableServiceClient.GetTableClient(tableName: typeof(T).Name); //Name convention of table name
        
        public Task<T> get(string id)
        {
            throw new NotImplementedException();
        }

        public Task<T> get(Guid id)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<T> search(Query<T> query)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<T> search<TQuery>() where TQuery : Query<T>
        {
            throw new NotImplementedException();
        }

        public async Task save(T entity)
        {
            await TableClient.CreateIfNotExistsAsync();
            await TableClient.AddEntityAsync(entity);
        }
    }
}
