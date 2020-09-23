using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OperationResult;

[assembly: InternalsVisibleTo("EventBus.EventLog.EntityFrameworkCore.Extensions.DependencyInjection")]
namespace EventBus.EventLog
{
    public class EventLogPublisher : IEventLogPublisher
    {
        private readonly IEventLogService _eventLogService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<EventLogPublisher> _logger;
        private readonly IEventLogUnitOfWork _eventLogUnitOfWork;

        public EventLogPublisher(IEventLogService eventLogService, IEventPublisher eventPublisher, 
            ILogger<EventLogPublisher> logger, IEventLogUnitOfWork eventLogUnitOfWork)
        {
            _eventLogService = eventLogService;
            _eventPublisher = eventPublisher;
            _logger = logger;
            _eventLogUnitOfWork = eventLogUnitOfWork;
        }

        public async Task<Result> PublishPendingEventLogs(Guid transactionId)
        {
            using (_logger.BeginScope($"transactionId: {transactionId}"))
            {
                var eventLogs = RetrievePendingEventLogsInTransaction(transactionId);
                try
                {
                    await _eventPublisher.PublishManyAsync(eventLogs.Select(e => new EventPublishRequest(e.Content, e.EventId, e.EventName)).ToArray());
                    SetAllEventsAsSent(eventLogs);
                    return Result.Success();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        $"Failed to publish events {eventLogs.Select(e => e.EventName)} with ids {eventLogs.Select(e => e.Id)}");
                    SetAllEventsAsError(eventLogs, ex);
                    return Result.Error(ex);
                }
                finally
                {
                    await _eventLogUnitOfWork.SaveChangesAsync();
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
            var eventLogs = _eventLogService.EventLogs
                .Where(e => e.TransactionId == transactionId && e.Status == Status.Pending);
            _logger.LogInformation($"{eventLogs.Count()} pending events were retrieved");
            return eventLogs.ToArray();
        }
    }
}