using MongoDB.Bson.Serialization;

namespace EventBus.EventLog.MongoDb
{
    public static class EventLogSchemaMapper
    {
        public static void RegisterEventLog()
        {
            BsonClassMap.RegisterClassMap<EventLog>(cm =>
            {
                cm.AutoMap();
                cm.UnmapMember(e => e.Content);
            });
        }
    }
}