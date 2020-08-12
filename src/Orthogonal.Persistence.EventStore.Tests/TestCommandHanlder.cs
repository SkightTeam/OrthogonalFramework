using System.Threading.Tasks;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore.Tests
{
    public class TestCommandHandler:CommandHandler<TestCommand>
    
    {
        public Task handler(TestCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}