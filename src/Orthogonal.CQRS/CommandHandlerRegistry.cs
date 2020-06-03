namespace Orthogonal.CQRS
{
    public interface CommandHandlerRegistry
    {
        void register(CommandHandler handler);
    }
}