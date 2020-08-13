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
using NSubstitute;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore.Tests
{
    public  class CommandBusSpecs : WithSubject<CommandBus>
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
            user = new UserCredentials("admin", "changeit");

        };
        private Because of = () =>
        {
            Task.Run(async () =>
            {
                await connection.ConnectAsync();
                await Task.Delay(1000);
                command = new TestCommand(){Name= new Random().Next(10000).ToString() };
                handler=An<TestCommandHandler>();
               
                Subject.register(handler);
                Subject.create(user);
                await Subject.start();
                await Subject.publish(command);
            }).Wait();
            Thread.Sleep(500);
        };

        private It should_call_commandHandler = () =>
        {
            handler.Received().handler(Arg.Is<TestCommand>(x=>x.Name == command.Name));
        };

        private static TestCommand command;
        private static CommandHandler<TestCommand> handler;
        private static IEventStoreConnection connection;
        private static UserCredentials user;
    }
}
