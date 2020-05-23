using System;
using System.Collections.Generic;
using System.Text;

namespace Orthogonal.Persistence.EventStore
{
    public interface Event
    {
        Guid SourceId { get; }
    }
}
