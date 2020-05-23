using System;

namespace Orthogonal.CQRS
{
    public interface Event
    {
        Guid SourceId { get; }
    }
}
