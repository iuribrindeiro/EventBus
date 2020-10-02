using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventBus.EventLog.MongoDb
{
    public class MongoDbContext : IEventLogUnitOfWork
    {
        private readonly EventLogMongoDbOptions _mongoDbOptions;
        private readonly IClientSession _mongoSession;

        public IMongoCollection<EventLog> EventLogsCollection { get; }

        public MongoDbContext(IMongoSessionProvider sessionProvider, IOptions<EventLogMongoDbOptions> eventLogMongoDbOptions)
        {
            _mongoSession = sessionProvider.MongoSession;
            _mongoDbOptions = eventLogMongoDbOptions.Value;
            EventLogsCollection = _mongoSession.Client.GetDatabase(_mongoDbOptions.DatabaseName).GetCollection<EventLog>(_mongoDbOptions.CollectionName);
        }

        public async Task<Guid> SaveChangesAsync(Guid transactionId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("TransactionId", Guid.Empty);
            var collection = _mongoSession.Client
                .GetDatabase(_mongoDbOptions.DatabaseName)
                .GetCollection<BsonDocument>(_mongoDbOptions.CollectionName);
            var update = Builders<BsonDocument>.Update.Set("TransactionId", transactionId);

            await collection.UpdateManyAsync(filter, update);
            await _mongoSession.CommitTransactionAsync();
            return transactionId;
        }

        public Task SaveChangesAsync() 
            => _mongoSession.CommitTransactionAsync();
    }
}