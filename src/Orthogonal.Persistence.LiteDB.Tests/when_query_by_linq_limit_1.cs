using System.Linq;
using FluentAssertions;
using Machine.Specifications;

namespace Orthogonal.Persistence.LiteDB.Tests
{
    class when_query_by_linq_limit_1 : RepositoryImplSpecs
    {
        private Because of = () =>
        {
            ResultCount = Subject.search(new LiteQueryImpl<TestEntity>(
                f => f.Where(x => x.Name == "Property1").Limit(1)
            )).CountAsync().Result;
        };

        private It should_retuern_only_1 = () => ResultCount.Should().Be(1);

        public static int ResultCount { get; set; }
    }
}