namespace Orthogonal.Persistence.EventStore
{
    public interface Memento
    {
        int Version { get; }
    }
}