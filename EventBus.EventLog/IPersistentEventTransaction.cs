using System;
using System.Threading.Tasks;

namespace EventBus.EventLog
{
    public interface IPersistentEventTransaction
    {
        Task<Guid> SaveChangesWithEventLogsAsync();

        void AddEvent(Event @event);
    }
}