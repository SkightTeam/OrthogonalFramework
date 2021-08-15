using FluentAssertions;
using Machine.Specifications;

namespace Orthogonal.Persistence.LiteDB.Tests
{
    class when_get_by_guid_id : RepositoryImplGuidSpecs
    {
        private Because of = () =>
        {
            Result = Subject.get(NewEntity1.Id).Result;
        };

        private It should_return = () => Result.Should().NotBeNull();
        private It result_id_should_T001 = () => Result.Name.Should().Be("Item1");

        public static TestEntityWithGuid Result { get; set; }
    }
}