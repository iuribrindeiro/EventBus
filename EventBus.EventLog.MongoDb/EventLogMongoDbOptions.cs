namespace EventBus.EventLog.MongoDb
{
    public class EventLogMongoDbOptions
    {
        public string DatabaseName { get; set; } = "EventLogs";
        public string CollectionName { get; set; } = "EventLogs";
    }
}