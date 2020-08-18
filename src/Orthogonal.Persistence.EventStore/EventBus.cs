using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using Orthogonal.CQRS;
using EventHandler = Orthogonal.CQRS.EventHandler;

namespace Orthogonal.Persistence.EventStore
{
    public class EventBus  : EventHandlerRegistry
    {
        private static string Stream = "EventBus";
        private static string Subscription = "EventReceive";
        private static string Projection = "EventProjection";
        private ConcurrentDictionary<Type, ICollection<EventHandler>> event_handlers;
        private readonly Manager manager;
        public EventBus(Manager manager)
        {
            this.manager = manager;
            event_handlers=new ConcurrentDictionary<Type, ICollection<EventHandler>>();
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


        public async Task create()
        {
            var projections = await manager.ProjectionsManager.ListContinuousAsync(manager.Admin);
            if (projections.All(x => x.Name != Projection))
            {
                await manager
                    .ProjectionsManager
                    .CreateContinuousAsync(Projection, query.Replace("{Stream}", Stream), manager.Admin);
            }

            var subscriptions = await manager.PersistentSubscriptionsManager.List(manager.Admin);
            if (subscriptions.All(x => x.GroupName != Subscription))
            {
                var setting = create_subscription();
                await manager.Connection.CreatePersistentSubscriptionAsync(Stream, Subscription, setting, manager.Admin);


            }
        }

        private string query =
            @"fromAll()
                .when({
                    $any: function(s, e) {
                    if( e.streamId !='EventBus' 
                        && e.streamId != 'CommandBus'
                        && e.eventType 
                        && !e.eventType.startsWith('$')) {
                         linkTo('{Stream}', e)
                    }
                }})";

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
                Subscribe(manager.Connection);
                manager.Connection.Connected += OnConnected;
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
