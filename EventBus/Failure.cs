using System;
using System.Runtime.CompilerServices;

namespace EventBus
{
    public class Failure<T> where T : Event
    {
        public Failure(T @event, int retryAttempt, Exception exception)
        {
            Event = @event;
            RetryAttempt = retryAttempt;
            Exception = exception;
        }

        public T Event { get; }
        public int RetryAttempt { get; }
        public Exception Exception { get; }
    }
}