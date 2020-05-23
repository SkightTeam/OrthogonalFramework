namespace Orthogonal.Persistence.EventStore.Tests
{
    public class EntityCreated : VersionedEventBase
    {
        public string Name { get; set; }
    }
}