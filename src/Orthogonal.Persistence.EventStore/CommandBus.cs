using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore
{
    public class CommandBus : CommandPublisher, CommandHandlerRegistry
    {
        private readonly IEventStoreConnection event_store_connection;

        public CommandBus(IEventStoreConnection eventStoreConnection)
        {
            event_store_connection = eventStoreConnection;
        }

        public Task publish(Command command)
        {
            throw new NotImplementedException();
        }

        public void register(CommandHandler handler)
        {
            throw new NotImplementedException();
        }
    }
}
