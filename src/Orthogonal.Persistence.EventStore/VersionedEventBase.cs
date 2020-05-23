using System;

namespace Orthogonal.Persistence.EventStore
{
    public abstract class VersionedEventBase : VersionedEvent
    {
        public Guid SourceId { get; set; }

        public int Version { get; set; }
    }
}