using System;
using NUnit.Framework;

namespace EventBus.Test
{
    public class FailureTest
    {
        [Test]
        public void WhenCreatingFailureShoudldSetPropertiesCorrectly()
        {
            var @event = new EventThatHasFailed();
            var exception = new Exception("Something wrong here...");
            var retryAttempt = 1;
            var failure = new Failure<EventThatHasFailed>(@event, retryAttempt, exception);

            Assert.AreEqual(retryAttempt, failure.RetryAttempt);
            Assert.AreSame(@event, failure.Event);
            Assert.AreSame(exception, failure.Exception);
        }

        public class EventThatHasFailed : Event
        {

        }
    }
}