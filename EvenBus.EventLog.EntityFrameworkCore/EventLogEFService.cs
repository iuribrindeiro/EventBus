using System.Linq;
using System.Threading.Tasks;

namespace EventBus.EventLog.EntityFrameworkCore
{
    public class EventLogEFService : IEventLogService
    {
        private readonly EventLogDbContext _eventLogDbContext;

        public EventLogEFService(EventLogDbContext eventLogDbContext) => _eventLogDbContext = eventLogDbContext;

        public IQueryable<EventLog> EventLogs => _eventLogDbContext.EventLogs;

        public void AddEvent(Event @event) 
            => _eventLogDbContext.EventLogs.Add(new EventLog(@event));

        public Task AddEventAsync(Event @event) 
            => _eventLogDbContext.EventLogs.AddAsync(new EventLog(@event)).AsTask();
    }
}