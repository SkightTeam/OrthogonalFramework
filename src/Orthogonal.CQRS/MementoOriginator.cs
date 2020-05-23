namespace Orthogonal.CQRS
{
    public interface MementoOriginator
    {
        Memento save_to_memento();
    }
}