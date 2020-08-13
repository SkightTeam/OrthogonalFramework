using System.Threading.Tasks;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore.Tests
{
    public class TestCommandHandler : CommandHandler<TestCommand>
    {
        public virtual Task handler(TestCommand command)
        {
            return Task.CompletedTask;
        }
    }
}