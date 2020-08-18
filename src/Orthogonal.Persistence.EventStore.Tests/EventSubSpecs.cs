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
             connection =             
               EventStoreConnection.Create(
                   ConnectionSettings.Create()
                       .KeepReconnecting()
                       .KeepRetrying()
                       .UseConsoleLogger()
                   ,
                   new Uri(
                       $"tcp://admin:changeit@127.0.0.1:1113"));
            Configure(r=>r.For<IEventStoreConnection>().Use(connection));
            repository=new RepositoryImpl<TestEntity>(connection, Subject, An<IMemoryCache>());
            user = new UserCredentials("admin", "changeit");
          
            Task.Run(async () =>
            {
                await connection.ConnectAsync();
                await Task.Delay(1000);
                handler=An<CQRS.EventHandler<EntityCreated>>();
                Subject.register(handler);
                await Subject.create(user);
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
        private static IEventStoreConnection connection;
        private static UserCredentials user;
        private static RepositoryImpl<TestEntity> repository;
    }
}
