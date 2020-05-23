using System.Text;
using FluentAssertions;
using Machine.Fakes;
using Machine.Specifications;
using NSubstitute;

namespace Orthogonal.Persistence.EventStore.Tests
{
    public  class RepositoryImplSpecs : WithSubject<RepositoryImpl<TestEntity>>
    {
        private Establish context = () =>
        {
            var config = An<ConnectionConfiguration>();
            config.Host.Returns("cloudview-eventstore.koreacentral.azurecontainer.io");
            config.Port.Returns(1113);
            config.User.Returns("admin");
            config.Password.Returns("changeit");
            Configure(r=>r.For<ConnectionConfiguration>().Use(config));
        };
        private Because of = () =>
        {
            var entity = new TestEntity("entityName");
            Subject.save(entity).Wait();
            Subject.save(entity).Wait();
            entity.set(1.2M);
            Subject.save(entity).Wait();

            savedEntity = Subject.get("").Result;
        };

        private It should_save_name = () => savedEntity.Name.Should().Be("entityName");
        private It should_save_value = () => savedEntity.Value.Should().Be(1.2M);

        private static TestEntity savedEntity;
    }
}
