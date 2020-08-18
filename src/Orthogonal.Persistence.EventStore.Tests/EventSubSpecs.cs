using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using FluentAssertions;
using Machine.Fakes;
using Machine.Specifications;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Orthogonal.CQRS;
using EventHandler = Orthogonal.CQRS.EventHandler;

namespace Orthogonal.Persistence.EventStore.Tests
{
    public  class EventBusSpecs : WithSubject<EventBus>
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
            var manager =new Manager(configuration);
            
            Configure(r=>r.For<Manager>().Use(manager));
            repository=new RepositoryImpl<TestEntity>(manager, Subject, An<IMemoryCache>());

            Task.Run(async () =>
            {
                await manager.Connection.ConnectAsync();
                await Task.Delay(1000);
                handler=An<CQRS.EventHandler<EntityCreated>>();
                Subject.register(handler);
                await Subject.create();
                await Subject.start();
                
            }).Wait();
        };
        private Because of = () =>
        {
            Task.Run(async () =>
            {
                entity = new TestEntity(new Random().Next(10000).ToString());
                await repository.save(entity);
                entity.set(1.2M);
                await repository.save(entity);
            }).Wait();
           
            Thread.Sleep(500);
        };

        private It should_call_event_handler = () =>
        {
              handler.Received().handle(Arg.Is<EntityCreated>(x=>x.Name==entity.Name)); };

        private static TestEntity entity;
        private static CQRS.EventHandler<EntityCreated> handler;
        private static UserCredentials user;
        private static RepositoryImpl<TestEntity> repository;
    }
}
