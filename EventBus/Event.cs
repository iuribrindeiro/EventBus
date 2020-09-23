using System;
using MediatR;
using Newtonsoft.Json;

namespace EventBus
{
    public abstract class Event : INotification
    {
        public Event()
        {
            Id = Guid.NewGuid();
            Date = DateTime.Now;
            Name = GetType().Name;
        }

        [JsonProperty("Id")]
        public virtual Guid Id { get; private set; }

        [JsonProperty("Date")]
        public virtual DateTime Date { get; private set; }

        [JsonProperty("Name")]
        public virtual string Name { get; private set; }
    }
}