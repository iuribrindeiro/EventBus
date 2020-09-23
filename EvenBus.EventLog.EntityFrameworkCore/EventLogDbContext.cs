using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventBus.EventLog.EntityFrameworkCore
{
    public class EventLogDbContext : DbContext, IEventLogDatabaseCreator, IEventLogUnitOfWork
    {
        public EventLogDbContext(DbContextOptions<EventLogDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventLog>().HasKey(p => p.Id);
            modelBuilder.Entity<EventLog>().Ignore(p => p.Event);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<EventLog> EventLogs { get; set; }

        public Task SaveChangesAsync() => SaveChangesAsync();
        
        private void SetCurrentTransactionId(Guid transactionId)
        {
            var eventLogs = ChangeTracker.Entries()
                .Where(e => e.Entity is EventLog eventLog &&
                            eventLog.TransactionId == Guid.Empty);
            foreach (var eventLog in eventLogs)
                eventLog.CurrentValues["TransactionId"] = transactionId;
        }

        public Task<int> SaveChangesAsync(Guid transactionId)
        {
            SetCurrentTransactionId(transactionId);
            return base.SaveChangesAsync();
        }

        public void EnsureDatabaseCreated() 
            => Database.EnsureCreated();
    }
}