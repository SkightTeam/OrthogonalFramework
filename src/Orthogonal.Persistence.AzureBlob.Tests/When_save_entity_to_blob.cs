using FluentAssertions;
using Machine.Specifications;

namespace Orthogonal.Persistence.AzureBlob.Tests
{
    public class When_save_entity_to_blob : RepositoryImplSpec
    {
        private Establish context = () => Entity = new TestEntity
        {
            Id="id005",
            Name = "Test",
            Value = 3
        };

        private Because of = () =>
        {
            Subject.save(Entity).Wait();
            savedEntity = Subject.get(Entity.Id).Result;
        };

        private Cleanup that = () => Subject.delete(Entity.Id);

        private It should_saved = () => savedEntity.Should().NotBeNull();
        private It should_saved_correctly = () =>savedEntity.Should().BeEquivalentTo(Entity);
        private static TestEntity Entity;
        private static TestEntity savedEntity;
    }
}