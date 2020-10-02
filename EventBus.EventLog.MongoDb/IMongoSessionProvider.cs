using MongoDB.Driver;

namespace EventBus.EventLog.MongoDb
{
    public interface IMongoSessionProvider
    {
        IClientSession MongoSession { get; }
    }
}