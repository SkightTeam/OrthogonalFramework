using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Machine.Specifications;

namespace Orthogonal.Persistence.AzureBlob.Tests
{
    public class When_search_entity_by_find_all : RepositoryImplSpec
    {
        private Establish context = () =>
        {
            Entity = new TestEntity
            {
                Id = "id005",
                Name = "Test",
                Value = 3
            };
            Subject.save(Entity).Wait();
        };

        private Because of = () => Result = Subject.search<FindAll<TestEntity>>().ToListAsync().Result;

        private It should_return_items = () => Result.Should().HaveCountGreaterThan(0);
        private Cleanup that = () => Subject.delete(Entity.Id);


        private static IEnumerable<TestEntity> Result;
        private static TestEntity Entity;
}
}
