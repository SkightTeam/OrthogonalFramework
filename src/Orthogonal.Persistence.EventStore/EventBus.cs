using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Orthogonal.CQRS;
using EventHandler = Orthogonal.CQRS.EventHandler;

namespace Orthogonal.Persistence.EventStore
{
    public class EventBus  : EventPublisher, EventHandlerRegistry
    {
        private static string Stream = "EventBus";
        private static string Subscription = "EventReceive";
        private ConcurrentDictionary<Type, ICollection<EventHandler>> event_handlers;
        private readonly IEventStoreConnection event_store_connection;

        public EventBus(IEventStoreConnection eventStoreConnection)
        {
            event_store_connection = eventStoreConnection;
            event_handlers=new ConcurrentDictionary<Type, ICollection<EventHandler>>();
        }


        public async Task publish(params Event[] events)
        {
            await event_store_connection.AppendToStreamAsync(
                Stream,
                ExpectedVersion.Any,
                events.Select(x=>x.create_event_data()).ToArray());
        }

        public void register(EventHandler handler)
        {
            var genericHandler = typeof(CQRS.EventHandler<>);           
            var supportType = handler.GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericHandler)
                .Select(i => i.GetGenericArguments()[0])
                .ToList();
            
            // Register this handler for each of the handled types.
            foreach (var type in supportType)
            {
                var type_handlers = event_handlers.GetOrAdd(type, x=> new HashSet<EventHandler>());
                type_handlers.Add(handler);
            }
        }


        public async Task create(UserCredentials user)
        {
            try
            {
                var setting = create_subscription();
                await event_store_connection.CreatePersistentSubscriptionAsync(Stream, Subscription, setting, user);
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(InvalidOperationException)
                    || ex.Message != $"Subscription group {Subscription} on stream {Stream} already exists")
                {
                    throw;
                }
            }
        }

        private PersistentSubscriptionSettings create_subscription()
        {
            return PersistentSubscriptionSettings.Create()
                .ResolveLinkTos()
                .StartFromBeginning();
        }

        public async Task start()
        {
            await Task.Run(() =>
            {
                Subscribe(event_store_connection);
                event_store_connection.Connected += OnConnected;
            });
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
                    Console.WriteLine($" process event : {x.Event.EventType}{x.Event.EventNumber} ");
                    var evt = x.extract_data();
                    if (event_handlers.TryGetValue(evt.GetType(), out var handlers))
                    {
                        Parallel.ForEach(handlers, handler => ((dynamic)handler).handle((dynamic) evt));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error process event: {x.Event.EventType}{x.Event.EventNumber} ");
                    Console.WriteLine(e.StackTrace);
                }
            });
        }
    }
}
