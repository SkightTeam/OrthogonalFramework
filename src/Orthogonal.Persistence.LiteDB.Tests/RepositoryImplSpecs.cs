using Machine.Fakes;
using Machine.Specifications;
using NSubstitute;

namespace Orthogonal.Persistence.LiteDB.Tests
{
    class RepositoryImplSpecs : WithSubject<RepositoryImpl<TestEntity>>
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
}
