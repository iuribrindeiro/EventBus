using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace EventBus.EventLog.MongoDb
{
    public class MongoEventLogService : IEventLogService
    {
        private readonly MongoDbContext _mongoDbContext;

        public MongoEventLogService(MongoDbContext mongoDbContext) 
            => _mongoDbContext = mongoDbContext;

        public IQueryable<EventLog> EventLogs =>
            _mongoDbContext.EventLogsCollection.AsQueryable();

        public void AddEvent(Event @event)
            => _mongoDbContext.EventLogsCollection.InsertOne(new EventLog(@event));

        public Task AddEventAsync(Event @event)
            => _mongoDbContext.EventLogsCollection.InsertOneAsync(new EventLog(@event));
    }
}