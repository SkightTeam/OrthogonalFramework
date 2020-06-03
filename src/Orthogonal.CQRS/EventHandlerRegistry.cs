namespace Orthogonal.CQRS
{
    public interface EventHandlerRegistry
    {
        void register(EventHandler handler);
    }
}