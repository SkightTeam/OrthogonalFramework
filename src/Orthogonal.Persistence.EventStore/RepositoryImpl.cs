using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orthogonal.Persistence.EventStore
{
    public class RepositoryImpl<T> : Repository<T>
    {
        public Task<T> get(string id)
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

        public Task save(T t)
        {
            throw new NotImplementedException();
        }
    }
}
