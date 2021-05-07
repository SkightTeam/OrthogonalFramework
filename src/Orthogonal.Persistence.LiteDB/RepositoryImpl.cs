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
            throw new System.NotImplementedException();
        }

        public async IAsyncEnumerable<T> search(Query<T> query)
        {
            using var db = new LiteDatabase(configuration.DatabaseLoclation);
            var col = db.GetCollection<T>();
            switch (query)
            {
                case LinqQuery<T> linqQuery:
                    foreach (var item in col.Find(linqQuery.Predicate))
                    {
                        yield return item;
                    }
                    break;
                default:
                    yield break;
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
               using var db = new LiteDatabase(configuration.DatabaseLoclation);
               var col = db.GetCollection<T>();
               col.Upsert(entity);
           });
        }
    }
}