using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EventBus.EventLog.EntityFrameworkCore
{
    internal class PersistentEventTransaction : IPersistentEventTransaction
    {
        private readonly IDbContextApplicationProvider _dbContextApplicationProvider;
        private readonly EventLogDbContext _eventLogDbContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<PersistentEventTransaction> _logger;

        public PersistentEventTransaction(
            IDbContextApplicationProvider dbContextApplicationProvider, 
            EventLogDbContext eventLogDbContext, IEventPublisher eventPublisher, ILogger<PersistentEventTransaction> logger)
        {
            _dbContextApplicationProvider = dbContextApplicationProvider;
            _eventLogDbContext = eventLogDbContext;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<Guid> SaveChangesWithEventLogsAsync()
        {
            var strategy = _dbContextApplicationProvider.DbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = _dbContextApplicationProvider.DbContext.Database.BeginTransaction();
                await Task.WhenAll(
                    _dbContextApplicationProvider.DbContext.SaveChangesAsync(), 
                    _eventLogDbContext.SaveChangesAsync(transaction.TransactionId));
                transaction.Commit();
                return transaction.TransactionId;
            });
        }

        public void AddEvent(Event @event)
            => _eventLogDbContext.Add(new EventLog(@event));
    }
}
