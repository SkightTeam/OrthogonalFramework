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
            NewEntity = new TestEntity
            {
                Id = "T001",
                Name = "Property1",
                Value = 2
            };
        };

        private Because of = () =>
        {
            Subject
                .save(NewEntity)
                .Wait();
            SavedEntity = Subject.search(new LingQueryImpl<TestEntity>(x => x.Id == "T001"))
                .FirstOrDefaultAsync().Result;
        };

        private It should_save_success = () => SavedEntity.Should().NotBeNull();
        private It saved_entity_Id_should_save_success = () => SavedEntity.Id.Should().Be("T001");
        private It saved_entity_name_should_save_success = () => SavedEntity.Name.Should().Be("Property1");
        private It saved_entity_value_should_save_success = () => SavedEntity.Value.Should().Be(2);

        public static TestEntity NewEntity { get; set; }
        public static TestEntity SavedEntity { get; set; }
    }
}
