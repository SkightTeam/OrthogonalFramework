namespace Orthogonal.Persistence.EventStore
{
    public interface MementoOriginator
    {
        Memento save_to_memento();
    }
}