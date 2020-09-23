using System;

namespace EventBus
{
    public sealed class EventPublishRequest
    {
        public EventPublishRequest(string eventBody, Guid eventId, string eventName)
        {
            EventBody = eventBody;
            EventId = eventId;
            EventName = eventName;
        }

        public string EventBody { get; }
        public Guid EventId { get; }
        public string EventName { get; }
    }
}