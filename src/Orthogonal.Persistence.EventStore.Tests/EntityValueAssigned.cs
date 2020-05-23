using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore.Tests
{
    public class EntityValueAssigned : VersionedEventBase
    {
        public decimal Value { get; set; }
    }
}