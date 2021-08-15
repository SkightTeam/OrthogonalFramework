using System;
using LiteDB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orthogonal.Persistence.LiteDB
{
    public class RepositoryImpl<T> : Repository<T>
    {
        private LiteDBClientConfiguration configuration;
        public RepositoryImpl(LiteDBClientConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Task<T> get(string id)
        {
            using var db = new LiteDatabase($"FileName={configuration.DatabaseLoclation};Connection=shared");
            var col = db.GetCollection<T>();
            return Task.FromResult(col.FindById(id));
        }

        public Task<T> get(Guid id)
        {
            using var db = new LiteDatabase($"FileName={configuration.DatabaseLoclation};Connection=shared");
            var col = db.GetCollection<T>();
            return Task.FromResult(col.FindById(id));
        }

        public async IAsyncEnumerable<T> search(Query<T> query)
        {
            using var db = new LiteDatabase($"FileName={configuration.DatabaseLoclation};Connection=shared");
            var col = db.GetCollection<T>();
            IEnumerable<T> result;
            switch (query)
            {
                case LinqQuery<T> linqQuery:
                    result = col.Find(linqQuery.Predicate);
                    break;
                case LiteQuery<T> liteQuery:
                    result= liteQuery.Filter(col.Query()).ToEnumerable();
                    break;
                default:
                    yield break;
            }

            foreach (var item in result)
            {
                yield return item;
            }
        }

        public IAsyncEnumerable<T> search<TQuery>() where TQuery : Query<T>
        {
            throw new System.NotImplementedException();
        }

        public async Task save(T entity)
        {
            await Task.Run(() =>
           {
               using var db = new LiteDatabase($"FileName={configuration.DatabaseLoclation};Connection=shared");
               var col = db.GetCollection<T>();
               col.Upsert(entity);
           });
        }
    }
}