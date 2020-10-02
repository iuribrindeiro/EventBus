using System;
using System.Threading.Tasks;

namespace EventBus.EventLog.MongoDb
{
    public class MongoPersistentEventTransaction : IPersistentEventTransaction
    {
        private readonly IMongoSessionProvider _mongoSessionProvider;
        private readonly MongoDbContext _mongoDbContext;

        public MongoPersistentEventTransaction(IMongoSessionProvider mongoSessionProvider, MongoDbContext mongoDbContext)
        {
            _mongoSessionProvider = mongoSessionProvider;
            _mongoDbContext = mongoDbContext;
        }

        public Task<Guid> SaveChangesWithEventLogsAsync()
        {
            var transactionId = _mongoSessionProvider.MongoSession.ServerSession.Id.AsGuid;
            return _mongoDbContext.SaveChangesAsync(transactionId);
        }
    }
}