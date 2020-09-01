using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

[assembly:InternalsVisibleTo("EventBus.EventLog.EntityFrameworkCore.Extensions.DependencyInjection")]
namespace EventBus.EventLog.EntityFrameworkCore
{
    internal class EventLogPublisher : IEventLogPublisher
    {
        private readonly EventLogDbContext _eventLogDbContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<EventLogPublisher> _logger;

        public EventLogPublisher(EventLogDbContext eventLogDbContext, IEventPublisher eventPublisher, ILogger<EventLogPublisher> logger)
        {
            _eventLogDbContext = eventLogDbContext;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task PublishPendingEventLogs(Guid transactionId)
        {
            using (_logger.BeginScope($"transactionId: {transactionId}"))
            {
                var eventLogs = RetrievePendingEventLogsInTransaction(transactionId);
                try
                {
                    await _eventPublisher.PublishManyAsync(eventLogs.Select(e => e.Event).ToArray());
                    SetAllEventsAsSent(eventLogs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        $"Failed to publish events {eventLogs.Select(e => e.EventName)} with ids {eventLogs.Select(e => e.Id)}");
                    SetAllEventsAsError(eventLogs, ex);
                    throw;
                }
                finally
                {
                    await _eventLogDbContext.SaveChangesAsync();
                }
            }
        }

        private static void SetAllEventsAsError(EventLog[] eventLogs, Exception ex)
        {
            foreach (var eventLog in eventLogs)
                eventLog.SetEventAsErrorSending(ex.Message);
        }

        private static void SetAllEventsAsSent(EventLog[] eventLogs)
        {
            foreach (var eventLog in eventLogs)
                eventLog.SetEventAsSent();
        }

        private EventLog[] RetrievePendingEventLogsInTransaction(Guid transactionId)
        {
            _logger.LogInformation("Retrieving pending events");
            var eventLogs = _eventLogDbContext.EventLogs
                .Where(e => e.TransactionId == transactionId && e.Status == Status.Pending);
            _logger.LogInformation($"{eventLogs.Count()} pending events were retrieved");
            return eventLogs.ToArray();
        }
    }
}