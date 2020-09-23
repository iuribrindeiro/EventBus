using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventBus.EventLog
{
    public interface IEventLogService
    {
        IQueryable<EventLog> EventLogs { get; }
        void AddEvent(Event @event);
        Task AddEventAsync(Event @event);
    }
}