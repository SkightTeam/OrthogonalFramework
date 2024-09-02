using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Orthogonal.CQRS;
using EventHandler = Orthogonal.CQRS.EventHandler;

namespace Orthogonal.Persistence.AzureEventHub
{
    public class EventBus : EventPublisher, EventHandlerRegistry, IAsyncDisposable
    {
        private readonly string connection_string;
        private readonly string event_hub_name;
        private readonly string consumer_group;
        private readonly string check_point_connection_string;
        private readonly string check_point_container;
        private ConcurrentDictionary<Type, EventHandler> event_handlers;
        private EventHubBufferedProducerClient? producer;
        private EventProcessorClient? processor;

        public EventBus(AzureEventHubConfiguration configuration)
        {
            connection_string=configuration.ConnectionString;
            event_hub_name = configuration.EventHubName;
            check_point_connection_string = configuration.CheckPointConnectionString;
            check_point_container = configuration.CheckPointContainer;
            consumer_group = string.IsNullOrEmpty(configuration.ConsumerGroup)
                ? EventHubConsumerClient.DefaultConsumerGroupName
                : configuration.ConsumerGroup;
            event_handlers=new ConcurrentDictionary<Type, EventHandler>();
        }

        private EventHubBufferedProducerClient Producer
        {
            get
            {
                if (producer == null)
                {
                    producer = new EventHubBufferedProducerClient(
                        connection_string,
                        event_hub_name
                    );
                    producer.SendEventBatchFailedAsync += args =>
                    {
                        Debug.WriteLine($"Publishing failed for {args.EventBatch.Count} events.  Error: '{args.Exception.Message}'");
                        return Task.CompletedTask;
                    };

                    // The success handler is optional.

                    producer.SendEventBatchSucceededAsync += args =>
                    {
                        Debug.WriteLine($"{args.EventBatch.Count} events were published to partition: '{args.PartitionId}.");
                        return Task.CompletedTask;
                    };
                }

                return producer;
            }
        }

        private EventProcessorClient Processor
        {
            get
            {
                ensure_processor_initialized();
                return processor;
            }
        }

        private void ensure_processor_initialized()
        {
            if (processor == null)
            {
                processor = new EventProcessorClient(
                    new BlobContainerClient(check_point_connection_string, check_point_container),
                    consumer_group,
                    connection_string,
                    event_hub_name
                );
                processor.ProcessEventAsync += ProcessEventHandler;
                processor.ProcessErrorAsync += ProcessErrorHandler;
                processor.StartProcessing();
            }
        }

        public async Task publish(params Event[] events)
        {
            foreach (var @event in events)
            {
                var eventData = @event.create_event_data();
                await Producer.EnqueueEventAsync(eventData);
            }

            await Producer.FlushAsync();
        }
        async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            try
            {
                // Process the event data here
                var @event = eventArgs.Data.extract_data();
                if (event_handlers.TryGetValue(@event.GetType(), out var handler))
                {
                    ((dynamic)handler).handle((dynamic)@event);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing event: {ex.Message}");
            }
            finally
            {
                // Update checkpoint in storage so that we don't reprocess this event
                await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
            }

        }

        Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            Debug.Write($"Error processing event: {eventArgs.Exception.Message}");
            return Task.CompletedTask;
        }

        public void register(EventHandler handler)
        {
            var genericHandler = typeof(CQRS.EventHandler<>);
            var supportedEventTypes = handler.GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericHandler)
                .Select(i => i.GetGenericArguments()[0])
                .ToList();

            // Register this handler for each of the handled types.
            foreach (var eventType in supportedEventTypes)
            {
                if (!event_handlers.TryAdd(eventType, handler))
                {
                    throw new ArgumentException($"Event Handler {handler.GetType()} registered error: The {eventType} already registered.");
                }
            }

            ensure_processor_initialized();
        }

        public async ValueTask DisposeAsync()
        {
            if (producer != null)
            {
                await producer.FlushAsync();
                await producer.DisposeAsync();
            }

            if (processor != null)
            {
                try
                {
                    await processor.StopProcessingAsync();
                }
                finally
                {
                    // To prevent leaks, the handlers should be removed when processing is complete.

                    processor.ProcessEventAsync -= ProcessEventHandler;
                    processor.ProcessErrorAsync -= ProcessErrorHandler;
                }
            }
        }
    }
}