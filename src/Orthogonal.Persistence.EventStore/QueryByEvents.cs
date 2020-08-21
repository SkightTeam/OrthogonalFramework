using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore
{
    public interface QueryByEvents<T> : Query<T>
    {
        IAsyncEnumerable<T> apply(IAsyncEnumerable<VersionedEvent> events);
    }
}
