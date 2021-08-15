using Machine.Fakes;
using Machine.Specifications;
using NSubstitute;

namespace Orthogonal.Persistence.LiteDB.Tests
{
    class RepositoryImplGuidSpecs : WithSubject<RepositoryImpl<TestEntityWithGuid>>
    {
        private Establish context = () =>
        {
            The<LiteDBClientConfiguration>().DatabaseLoclation.Returns("UnitTestDatabase.db");
            NewEntity1 = new TestEntityWithGuid
            {
                Name = "Item1",
                Value = 2
            };
            NewEntity2 = new TestEntityWithGuid
            {
                Name = "Item2",
                Value = 3
            };
            Subject
                .save(NewEntity1)
                .Wait();
            Subject
                .save(NewEntity2)
                .Wait();
        };


        protected static TestEntityWithGuid NewEntity1 { get; set; }
        protected static TestEntityWithGuid NewEntity2 { get; set; }
    }
}