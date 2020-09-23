using System;
using System.Threading.Tasks;
using OperationResult;

namespace EventBus.EventLog
{
    public interface IEventLogPublisher
    {
        Task<Result> PublishPendingEventLogs(Guid transactionId);
    }
}