using Newtonsoft.Json;
using System;

namespace EventBus.EventLog
{
    public class EventLog
    {
        private EventLog() {}

        public EventLog(Event @event)
        {
            Id = Guid.NewGuid();
            Content = JsonConvert.SerializeObject(@event, new JsonSerializerSettings());
            Status = Status.Pending;
            DateCreated = @event.Date;
            EventName = @event.GetType().Name;
            EventId = @event.Id;
            Event = @event;
        }

        public virtual void SetEventAsSent()
        {
            Status = Status.Sent;
            DateSent = DateTime.Now;
        }

        public virtual void SetEventAsErrorSending(string error)
        {
            Status = Status.ErrorSending;
            Error = error;
        }

        public Guid Id { get; private set; }
        public string Content { get; private set; }
        public Status Status { get; private set; }
        public DateTime DateSent { get; private set; }
        public DateTime DateCreated { get; private set; }
        public string EventName { get; private set; }
        public Guid EventId { get; private set; }
        public string Error { get; private set; }
        public Guid TransactionId { get; private set; }
        public virtual Event Event { get; private set; }
    }
}