using System.Threading.Tasks;

namespace Orthogonal.CQRS
{
    public interface CommandHandler
    {

    }

    public interface CommandHandler<T> : CommandHandler
        where T : Command
    {
        Task handler(T command);
    }
}