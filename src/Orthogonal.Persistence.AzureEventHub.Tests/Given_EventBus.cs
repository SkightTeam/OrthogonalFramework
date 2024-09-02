using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.AzureEventHub.Tests
{
    [TestClass]
    public class Given_EventBus
    {
        EventBus eventBus;

        [TestInitialize]
        public void SetUp()
        {
            var configuration = Substitute.For<AzureEventHubConfiguration>();
            configuration
                .ConnectionString
                .Returns(
                    "your connection string"
                  );
            configuration
                .EventHubName
                .Returns("test");
            configuration
                .CheckPointConnectionString
                .Returns(
                    "your checkpoint stroage connection string");
            configuration
                .CheckPointContainer
                .Returns("checkpoints");


            eventBus = new EventBus(configuration);
            eventBus.register(new MyEventHandler());
            Thread.Sleep(1000);

        }
        [TestMethod]
        public async Task Publish_Event()
        {
            var events = new Event[]{
                new MyEvent
                {
                    Name = "Hao"
                },
                new MyEvent
                {
                    Name = "Qian"
                }
            };
            await eventBus.publish(events);
            Thread.Sleep(30000);
        }

    }

    public class MyEventHandler : CQRS.EventHandler<MyEvent>
    {
        public Task handle(Event e)
        {
            Debug.WriteLine($"Received Event {JsonSerializer.Serialize(e, e.GetType())}");
            return Task.CompletedTask;
        }
    }

    public class MyEvent : Event
    {
        public Guid SourceId => Guid.NewGuid();
        public string Name { get; set; }
    }
}