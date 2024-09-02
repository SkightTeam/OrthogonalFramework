using Azure.Messaging.EventHubs;
using System.Text.Json;
using System.Text;
using System;
using Microsoft.Azure.Amqp.Framing;
using Orthogonal.CQRS;

namespace Orthogonal.Persistence.AzureEventHub
{
    public static class TypeExtensions
    {

        public static EventData create_event_data(this Event data)
        {
            var content = JsonSerializer.Serialize(data, data.GetType());
            var eventData = new EventData(
                Encoding.UTF8.GetBytes(content)
            )
            {
                Properties =
                {
                    { "EventType", data.GetType().AssemblyQualifiedName },
                    { "SourceId", data.SourceId }
                }
            };
            return eventData;
        }

        public static object extract_data(this EventData event_data)
        {
            var type = typeof(object);
            if (event_data.Properties.TryGetValue("EventType", out var typeName))
            {
                type = Type.GetType(typeName.ToString());
                
            }
            var data = Encoding.UTF8.GetString(event_data.Data.ToArray());
            return JsonSerializer.Deserialize(data, type);
        }
    }

}