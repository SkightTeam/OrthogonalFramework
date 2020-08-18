using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore
{
    public class RepositoryImpl<T> : Repository<T>
        where T : class, EventSourced
    {
        private readonly IEventStoreConnection event_store_connection;
        private readonly Manager manager;
        private readonly EventPublisher event_publisher;
        private readonly Func<IList<VersionedEvent>, T> entityFactory;
        private readonly Func<Memento, IList<VersionedEvent>, T> originatorEntityFactory;
        private readonly IMemoryCache cache;
        private readonly Action<string, T> cacheMementoIfApplicable;
        private readonly Func<string, Tuple<Memento, DateTime?>> getMementoFromCache;
        private readonly Action<string> markCacheAsStale;
        private bool is_event_store_connected;

        public RepositoryImpl(
            Manager manager,
            EventPublisher eventPublisher,
            IMemoryCache cache)
        {
            this.manager = manager;
            event_store_connection = manager.Connection;
            event_store_connection.Connected += on_event_store_connected;
            event_store_connection.Disconnected += on_event_store_disconnected;
            this.cache = cache;
            event_publisher = eventPublisher;
            // TODO: could be replaced with a compiled lambda to make it more performant
            var constructor = typeof(T).GetConstructor(new[] { typeof(IEnumerable<VersionedEvent>) });
            if (constructor == null)
            {
                throw new InvalidCastException(
                    $"Type {typeof(T)} must have a constructor with the following signature: .ctor(Guid, IEnumerable<IVersionedEvent>)");
            }
            entityFactory = (events) => (T)constructor.Invoke(new object[] { events });

            if (typeof(MementoOriginator).IsAssignableFrom(typeof(T)) && this.cache != null)
            {
                // TODO: could be replaced with a compiled lambda to make it more performant
                var mementoConstructor = typeof(T).GetConstructor(new[] { typeof(Memento), typeof(IEnumerable<VersionedEvent>) });
                if (mementoConstructor == null)
                {
                    throw new InvalidCastException(
                        "Type T must have a constructor with the following signature: .ctor(Guid, IMemento, IEnumerable<IVersionedEvent>)");
                }
                this.originatorEntityFactory = (memento, events) => (T)mementoConstructor.Invoke(new object[] { memento, events });
                this.cacheMementoIfApplicable = (key, originator) =>
                {
                    var memento = ((MementoOriginator)originator).save_to_memento();
                    this.cache.Set(
                        key,
                        new Tuple<Memento, DateTime?>(memento, DateTime.UtcNow),
                        DateTimeOffset.UtcNow.AddMinutes(30));
                };
                this.getMementoFromCache = key => (Tuple<Memento, DateTime?>)this.cache.Get(key);
                this.markCacheAsStale = key =>
                {

                    var item = (Tuple<Memento, DateTime?>)this.cache.Get(key);
                    if (item != null && item.Item2.HasValue)
                    {
                        item = new Tuple<Memento, DateTime?>(item.Item1, null);
                        this.cache.Set(
                            key,
                            item,
                            DateTimeOffset.UtcNow.AddMinutes(30));
                    }
                };
            }
            else
            {
                // if no cache object or is not a cache originator, then no-op
                this.cacheMementoIfApplicable = (key, value) => { };
                this.getMementoFromCache = id => null;
                this.markCacheAsStale = id => { };
            }

        }

        private void on_event_store_disconnected(object sender, ClientConnectionEventArgs e)
        {
            is_event_store_connected = false;
        }

        private void on_event_store_connected(object sender, ClientConnectionEventArgs e)
        {
            is_event_store_connected = true;
        }

        public async Task<T> get(string id)
        {
            var key = generate_key(id);
            var cachedMemento = getMementoFromCache(key);
            if (cachedMemento != null && cachedMemento.Item1 != null)
            {
                IList<VersionedEvent> deserialized;
                if (!cachedMemento.Item2.HasValue || cachedMemento.Item2.Value < DateTime.UtcNow.AddSeconds(-1))
                {
                    deserialized = await read_stream(key, cachedMemento.Item1.Version + 1);
                }
                else
                {
                    // if the cache entry was updated in the last seconds, then there is a high possibility that it is not stale
                    // (because we typically have a single writer for high contention aggregates). This is why we optimistically avoid
                    // getting the new events from the EventStore since the last memento was created. In the low probable case
                    // where we get an exception on save, then we mark the cache item as stale so when the command gets
                    // reprocessed, this time we get the new events from the EventStore.
                    deserialized = new List<VersionedEvent>();
                }

                return originatorEntityFactory.Invoke(cachedMemento.Item1, deserialized);
            }
            else
            {
                var deserialized = await read_stream(key, 0);
                if (deserialized.Any())
                {
                    return entityFactory.Invoke(deserialized);
                }
            }
            return default;
        }

        private async Task<List<VersionedEvent>> read_stream(string key, long cursor)
        {
            bool isEnd;
            var domainEvents = new List<VersionedEvent>();
            do
            {
                var slice = await read_slice(key, cursor, 100);
                foreach (var e in slice.Events)
                {
                    var type = Type.GetType(e.Event.EventType);
                    var data = Encoding.UTF8.GetString(e.Event.Data);
                    var domainEvent = (VersionedEvent)JsonSerializer.Deserialize(data, type);
                    domainEvents.Add(domainEvent);
                }

                isEnd = slice.IsEndOfStream;
                cursor = slice.NextEventNumber;
            } while (!isEnd);

            return domainEvents;
        }

        public async Task save(T t)
        {
            // TODO: guarantee that only incremental versions of the event are stored
            var events = t.Events.OrderBy(e => e.Version).ToArray();
            if (events.Length > 0)
            {
                var key = generate_key(t.Id);
                try
                {
                    await write_stream(key, events);
                    cacheMementoIfApplicable(key, t);
                    t.events_persisted(events);
                }
                catch (WrongExpectedVersionException)
                {
                    markCacheAsStale(key);
                    throw;
                }
            }
        }

        private async Task<StreamEventsSlice> read_slice(string stream, long start, int size)
        {
            if(!is_event_store_connected)
                throw new ApplicationException("Event store is not connected.");
            return await event_store_connection.ReadStreamEventsForwardAsync(
                 stream, start, size, false);
        }

        private async Task write_stream(string stream, VersionedEvent[] events)
        {
            //TODO: if cannot connect to the event store, need cache changes or retry?
            if (!is_event_store_connected)
                throw new ApplicationException("Event store is not connected.");
            var eventData = events.Select(x=>x.create_event_data()).ToArray();
            await event_store_connection.AppendToStreamAsync(
                stream, events[0].Version - 1, eventData);
            await event_publisher.publish(events);
        }

        public IAsyncEnumerable<T> search(Query<T> query)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<T> search<TQuery>() where TQuery : Query<T>
        {
            throw new NotImplementedException();
        }

        private string generate_key(string id)
        {
            return string.Join('-', typeof(T).Name, id);
        }
    }
}
