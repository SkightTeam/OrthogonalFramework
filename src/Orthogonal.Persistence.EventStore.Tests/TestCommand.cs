using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore.Tests
{
    public class TestCommand:Command
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
    }
}