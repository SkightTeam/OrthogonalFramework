using System.Threading.Tasks;

namespace Orthogonal.CQRS
{
    interface EventPublisher
    {
        Task publish(Event e);
    }
}
