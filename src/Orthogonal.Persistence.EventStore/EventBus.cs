using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Orthogonal.CQRS;
using EventHandler = Orthogonal.CQRS.EventHandler;

namespace Orthogonal.Persistence.EventStore
{
    public class EventBus  : EventPublisher, EventHandlerRegistry
    {
        private readonly IEventStoreConnection event_store_connection;

        public EventBus(IEventStoreConnection eventStoreConnection)
        {
            event_store_connection = eventStoreConnection;
        }

        public Task publish(Event e)
        {
            throw new NotImplementedException();
        }

        public void register(EventHandler handler)
        {
            throw new NotImplementedException();
        }
    }
}
