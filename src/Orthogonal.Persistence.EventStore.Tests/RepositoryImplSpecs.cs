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
            var configuration = An<Configuration>();
            configuration.Server.Host.Returns("127.0.0.1");
            configuration.Server.TcpPort.Returns(1113);
            configuration.Server.HttpPort.Returns(2113);
            configuration.Admin.Name.Returns("admin");
            configuration.Admin.Password.Returns("changeit");
            configuration.Operator.Name.Returns("ops");
            configuration.Operator.Password.Returns("changeit");
            manager =new Manager(configuration);
            
            Configure(r=>r.For<Manager>().Use(manager));
           
        };
        private Because of = () =>
        {
            Task.Run(async () =>
            {
                await manager.Connection.ConnectAsync();
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
        private static Manager manager;
    }
}
