using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EventBus.Test")]
[assembly: InternalsVisibleTo("EventBus.RabbitMQ.Test")]
[assembly: InternalsVisibleTo("EventBus.RabbitMQ")]
namespace EventBus
{
    public interface ISubscription
    {
        Type EventType { get; }
        string EventName { get; }
    }

    public class Subscription<T> : ISubscription where T : Event
    {
        private string _eventName;
        private Type _eventType;
        internal RetryPolicyConfiguration<T> RetryPolicyConfiguration { get; }

        public string EventName => _eventType.Name;

        public Type EventType => _eventType;

        internal Subscription()
        {
            RetryPolicyConfiguration = new RetryPolicyConfiguration<T>();
            _eventType = typeof(T);
        }


        public void OnFailure(Action<RetryPolicyConfiguration<T>> config) 
            => config(RetryPolicyConfiguration);
    }
}