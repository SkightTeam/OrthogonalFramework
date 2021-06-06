using System.Linq;
using FluentAssertions;
using Machine.Specifications;

namespace Orthogonal.Persistence.LiteDB.Tests
{
    class when_query_by_linq1 : RepositoryImplSpecs
    {
        private Because of = () =>
        {
            ResultCount = Subject.search(new LiteQueryImpl<TestEntity>(
                f => f.Where(x => x.Name == "Property1")
            )).CountAsync().Result;
        };

        private It should_return_all_2 = () => ResultCount.Should().Be(2);

        public static int ResultCount { get; set; }
    }
}