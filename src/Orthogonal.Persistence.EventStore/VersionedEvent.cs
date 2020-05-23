using System.Collections.Generic;
using System.Text;

namespace Orthogonal.Persistence.EventStore
{
    public interface VersionedEvent : Event
    {
        int Version { get; set; }
    }
}
