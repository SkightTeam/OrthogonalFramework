using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orthogonal.Persistence
{
    public interface Repository<T>
    {
        Task<T> get(string id);
        Task<T> get(Guid id);
        IAsyncEnumerable<T> search(Query<T> query);
        IAsyncEnumerable<T> search<TQuery>() where TQuery : Query<T>;
        Task save(T t);

    }
}