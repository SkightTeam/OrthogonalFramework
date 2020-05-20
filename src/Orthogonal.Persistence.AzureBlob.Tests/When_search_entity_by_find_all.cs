using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Machine.Fakes;
using Machine.Specifications;
using NSubstitute;

namespace Orthogonal.Persistence.AzureBlob.Tests
{
    public class RepositoryImplSpec : WithSubject<RepositoryImpl<TestEntity>>
    {
        private Establish context = () => The<BlobClientConfiguration>().ConnectionString.Returns(
            @"DefaultEndpointsProtocol=https;AccountName=ersdb;AccountKey=O/algkCQluzAOSz0loY5i+lONZ+aiMlCVl9OHscCczEB7XconREZYuxB9EcriEExGnjP2hbm3U0/StqhY/w8fA==;EndpointSuffix=core.windows.net");
    }
    public class When_search_entity_by_find_all : RepositoryImplSpec
    {
       
        private Because of = () => Result = Subject.search<FindAll<TestEntity>>().ToListAsync().Result;

        private It should_return_items = () => Result.Should().HaveCountGreaterThan(0);

        private static IEnumerable<TestEntity> Result;
    }

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

    public class TestEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
