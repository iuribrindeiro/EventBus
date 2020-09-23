using Newtonsoft.Json;
using System;

namespace EventBus.EventLog
{
    public class EventLog
    {
        public EventLog(Event @event)
        {
            Id = Guid.NewGuid();
            Status = Status.Pending;
            DateCreated = @event.Date;
            EventName = @event.Name;
            EventId = @event.Id;
            Event = @event;
            Content = JsonConvert.SerializeObject(Event);
        }

        public void SetEventAsSent()
        {
            Status = Status.Sent;
            DateSent = DateTime.Now;
        }

        public void SetEventAsErrorSending(string error)
        {
            Status = Status.ErrorSending;
            Error = error;
        }

        public Guid Id { get; }
        public string Content { get; }
        public Status Status { get; private set; }
        public DateTime DateSent { get; private set; }
        public DateTime DateCreated { get; }
        public string EventName { get; set; }
        public Guid EventId { get; }
        public string Error { get; private set; }
        public Guid TransactionId { get; private set; }
        public Event Event { get; }
    }
}