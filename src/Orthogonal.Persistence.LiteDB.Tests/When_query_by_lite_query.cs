using System.Linq;
using FluentAssertions;
using Machine.Specifications;

namespace Orthogonal.Persistence.LiteDB.Tests
{
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