using System.Threading.Tasks;

namespace Orthogonal.CQRS
{
    public interface EventPublisher
    {
        Task publish(Event e);
    }
}
