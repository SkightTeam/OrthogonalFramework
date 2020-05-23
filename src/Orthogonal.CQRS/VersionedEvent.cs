namespace Orthogonal.CQRS
{
    public interface VersionedEvent : Event
    {
        int Version { get; set; }
    }
}
