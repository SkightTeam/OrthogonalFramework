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
        private readonly ConnectionConfiguration connection_configuration;
        private readonly Func<IList<VersionedEvent>, T> entityFactory;
        private readonly Func<Memento, IList<VersionedEvent>, T> originatorEntityFactory;
        private readonly IMemoryCache cache;
        private readonly Action<string, T> cacheMementoIfApplicable;
        private readonly Func<string, Tuple<Memento, DateTime?>> getMementoFromCache;
        private readonly Action<string> markCacheAsStale;

        public RepositoryImpl(ConnectionConfiguration connectionConfiguration, IMemoryCache cache)
        {
            this.connection_configuration = connectionConfiguration;
            this.cache = cache;
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

        private IEventStoreConnection connection;
        protected IEventStoreConnection Connection {
            get
            {
                if (connection == null)
                {
                    connection = EventStoreConnection.Create(
                        ConnectionSettings.Create()
                            .KeepReconnecting()
                            .KeepRetrying()
                            .UseConsoleLogger()
                        ,
                        new Uri(
                            $"tcp://{connection_configuration.User}:{connection_configuration.Password}@{connection_configuration.Host}:{connection_configuration.Port}"));
                    connection.ConnectAsync().Wait();
                }
                return connection;
            }
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
            return await Connection.ReadStreamEventsForwardAsync(
                 stream, start, size, false);
        }

        private async Task write_stream(string stream, VersionedEvent[] events)
        {
            var eventData = events.Select(create_event_data).ToArray();
            await Connection.AppendToStreamAsync(
                stream, events[0].Version - 1, eventData);
        }

        private EventData create_event_data(VersionedEvent versionedEvent)
        {
            return new EventData(
                Guid.NewGuid(),
                versionedEvent.GetType().AssemblyQualifiedName,
                true,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(versionedEvent, versionedEvent.GetType())),
               null);
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
