namespace Orthogonal.Persistence.AzureEventHub
{
    public interface AzureEventHubConfiguration
    {
        string ConnectionString { get; }
        string EventHubName { get; }
        string ConsumerGroup { get; }
        string CheckPointConnectionString { get; }
        string CheckPointContainer { get; }
    }
}