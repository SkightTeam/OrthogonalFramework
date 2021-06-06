using FluentAssertions;
using Machine.Fakes;
using Machine.Specifications;
using NSubstitute;
using System.Linq;

namespace Orthogonal.Persistence.LiteDB.Tests
{
    public class RepositoryImplSpecs : WithSubject<RepositoryImpl<TestEntity>>
    {
        private Establish context = () =>
        {
            The<LiteDBClientConfiguration>().DatabaseLoclation.Returns("UnitTestDatabase.db");
            NewEntity1 = new TestEntity
            {
                Id = "T001",
                Name = "Property1",
                Value = 2
            };
            NewEntity1 = new TestEntity
            {
                Id = "T002",
                Name = "Property1",
                Value = 3
            };
            Subject
                .save(NewEntity1)
                .Wait();
        };

       
        protected static TestEntity NewEntity1 { get; set; }
        protected static TestEntity NewEntity2 { get; set; }
    }
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
    class When_query_by_lite_query : RepositoryImplSpecs
    {
        private Because of = () =>
        {
           
            SavedEntity = Subject.search(new LingQueryImpl<TestEntity>(x => x.Id == "T001"))
                .FirstOrDefaultAsync().Result;
        };

        private It should_save_success = () => SavedEntity.Should().NotBeNull();
        private It saved_entity_Id_should_save_success = () => SavedEntity.Id.Should().Be("T001");
        private It saved_entity_name_should_save_success = () => SavedEntity.Name.Should().Be("Property1");
        private It saved_entity_value_should_save_success = () => SavedEntity.Value.Should().Be(2);

        protected static TestEntity SavedEntity { get; set; }
    }
}
