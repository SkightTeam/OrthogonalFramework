using System;

namespace Orthogonal.CQRS
{
    public abstract class VersionedEventBase : VersionedEvent
    {
        public Guid SourceId { get; set; }

        public int Version { get; set; }
    }
}