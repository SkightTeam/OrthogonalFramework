using System.Threading.Tasks;

namespace Orthogonal.CQRS
{
    public interface CommandPublisher
    {
        Task publish(params Command[] commands);
    }
}