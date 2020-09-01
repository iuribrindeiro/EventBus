using System;
using EventBus.Exceptions;

namespace EventBus
{
    public class RetryPolicyConfiguration<T> where T : Event
    {
        internal RetryPolicyConfiguration()
            => ForeverRetry = true;

        internal int MaxRetryTimes { get; private set; }
        internal bool ForeverRetry { get; private set; }
        internal Func<Failure<T>, TimeSpan> RetryFunc { get; private set; }

        public RetryPolicyConfiguration<T> RetryForTimes(int times)
        {
            MaxRetryTimes = times;
            ForeverRetry = false;
            return this;
        }

        public RetryPolicyConfiguration<T> RetryForever()
        {
            if (MaxRetryTimes > 0)
                throw new CantRetryForeverOverRetryTimesConfigurationException();

            ForeverRetry = true;
            return this;
        }

        public RetryPolicyConfiguration<T> ForEachRetryWait(TimeSpan waitTime)
        {
            RetryFunc = f => waitTime;
            return this;
        }

        public RetryPolicyConfiguration<T> ForEachRetryWait(Func<Failure<T>, TimeSpan> retryFunc)
        {
            RetryFunc = retryFunc;
            return this;
        }
    }
}