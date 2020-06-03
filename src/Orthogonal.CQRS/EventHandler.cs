using System.Threading.Tasks;

namespace Orthogonal.CQRS
{
    public interface EventHandler
    {

    }

    public interface EventHandler<T> : EventHandler
    {
        Task handle(Event e);
    }
}