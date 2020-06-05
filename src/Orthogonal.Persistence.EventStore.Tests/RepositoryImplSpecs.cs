using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
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
             connection =
               EventStoreConnection.Create(
                   ConnectionSettings.Create()
                       .KeepReconnecting()
                       .KeepRetrying()
                       .UseConsoleLogger()
                   ,
                   new Uri(
                       $"tcp://admin:changeit@cloudview-eventstore.koreacentral.azurecontainer.io:1113"));
            Configure(r=>r.For<IEventStoreConnection>().Use(connection));
        };
        private Because of = () =>
        {
            Task.Run(async () =>
            {
                await connection.ConnectAsync();
                await Task.Delay(1000);
                entity = new TestEntity(new Random().Next(10000).ToString());
                await Subject.save(entity);
                entity.set(1.2M);
                await Subject.save(entity);
                savedEntity= await Subject.get(entity.Id);
            }).Wait();
        };

        private It should_save_name = () => savedEntity.Name.Should().Be(entity.Name);
        private It should_save_value = () => savedEntity.Value.Should().Be(1.2M);

        private static TestEntity entity;
        private static TestEntity savedEntity;
        private static IEventStoreConnection connection;
    }
}
