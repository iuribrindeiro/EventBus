using System;
using System.Threading.Tasks;

namespace EventBus.EventLog
{
    public interface IEventLogPublisher
    {
        Task PublishPendingEventLogs(Guid transactionId);
    }
}