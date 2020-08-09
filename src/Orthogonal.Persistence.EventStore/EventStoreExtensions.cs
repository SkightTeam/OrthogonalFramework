using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using EventStore.ClientAPI;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.EventStore
{
    public static class EventStoreExtensions
    {
        public static EventData create_event_data(this object data)
        {
            return new EventData(
                Guid.NewGuid(),
                data.GetType().AssemblyQualifiedName,
                true,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, data.GetType())),
                null);
        }

        public static object extract_data(this ResolvedEvent event_data)
        {
            var type = Type.GetType(event_data.Event.EventType);
            var data = Encoding.UTF8.GetString(event_data.Event.Data); 
            return JsonSerializer.Deserialize(data, type);
        }
    }
}
