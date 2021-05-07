using FluentAssertions;
using Machine.Specifications;

namespace Orthogonal.Persistence.LiteDB.Tests
{
    class MSpecSelfTest
    {
        private It should_always_true = () => true.Should().BeTrue();
    }
}
