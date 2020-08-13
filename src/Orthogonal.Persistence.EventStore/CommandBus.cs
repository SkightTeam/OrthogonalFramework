using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore
{
    public class CommandBus : CommandPublisher, CommandHandlerRegistry
    {
        private static string Stream = "CommandBus";
        private static string Subscription = "CommandReceive";  
        private readonly IEventStoreConnection event_store_connection;
        private ConcurrentDictionary<Type, CommandHandler> command_handlers;

        public CommandBus(IEventStoreConnection eventStoreConnection)
        {
            event_store_connection = eventStoreConnection;
            command_handlers =new ConcurrentDictionary<Type, CommandHandler>();
        }

        public async Task publish(Command command)
        {
           await event_store_connection.AppendToStreamAsync(
                Stream,
                ExpectedVersion.Any,
                command.create_event_data());
        }

        public void register(CommandHandler handler)
        {
            var genericHandler = typeof(CommandHandler<>);           
            var supportedCommandTypes = handler.GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericHandler)
                .Select(i => i.GetGenericArguments()[0])
                .ToList();

            // Register this handler for each of the handled types.
            foreach (var commandType in supportedCommandTypes)
            {
                if (!command_handlers.TryAdd(commandType, handler))
                {
                    throw new ArgumentException($"Command Handler {handler.GetType()} registered error: The {commandType} already registered.");
                }
            }
        }

        public async Task start()
        {
            await Task.Run(() =>
            {
                Subscribe(event_store_connection);
                event_store_connection.Connected += OnConnected;
            });
        }

        public async Task create(UserCredentials user)
        {
            try
            {
                var setting = create_subscription();
                await event_store_connection.CreatePersistentSubscriptionAsync(Stream, Subscription, setting, user);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException.GetType() != typeof(InvalidOperationException)
                    && ex.InnerException?.Message != $"Subscription group {Subscription} on stream {Stream} already exists")
                {
                    throw;
                }
            }
               
            
        }

        private PersistentSubscriptionSettings create_subscription()
        {
           return PersistentSubscriptionSettings.Create()
                .DoNotResolveLinkTos()
                .StartFromBeginning();
        }

        private void OnConnected(object sender, ClientConnectionEventArgs clientConnectionEventArgs)
        {
            var connection = sender as IEventStoreConnection;
            Subscribe(connection);
        }

        private void Subscribe(IEventStoreConnection connection)
        {
            connection.ConnectToPersistentSubscriptionAsync(Stream, Subscription, (_, x) =>
            {
                try
                {
                    Console.WriteLine($" process command : {x.Event.EventType}{x.Event.EventNumber} ");
                    var command = x.extract_data();
                    if (command_handlers.TryGetValue(command.GetType(), out var handler))
                    {
                        ((dynamic)handler).handler((dynamic) command);
                    }
                    _.Acknowledge(x.Event.EventId);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error process command: {x.Event.EventType}{x.Event.EventNumber} ");
                    Console.WriteLine(e.StackTrace);
                }
            }, autoAck: false);
        }
    }
}
