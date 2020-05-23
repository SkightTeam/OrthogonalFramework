using System.Collections.Generic;

namespace Orthogonal.Persistence.EventStore
{
    public interface EventSourced
    {
        string Id { get; }
        int Version { get; }
        IEnumerable<VersionedEvent> Events { get; }
        void events_persisted(IEnumerable<VersionedEvent> events);
    }
}
