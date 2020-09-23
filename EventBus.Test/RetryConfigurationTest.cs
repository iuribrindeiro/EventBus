using System;
using EventBus.Exceptions;
using NUnit.Framework;

namespace EventBus.Test
{
    public class RetryConfigurationTest
    {
        [Test]
        public void WhenCreatingANewConfigurationShouldBeCreatedWithNoWaitTimeAndRetryForever()
        {
            var configuration = new RetryPolicyConfiguration<Event>();
            Assert.IsTrue(configuration.ForeverRetry);
            Assert.IsNull(configuration.RetryTime);
        }

        [Test]
        public void WhenConfiguringToRetryTimesShoudlSetPropertyCorrectly()
        {
            var configuration = new RetryPolicyConfiguration<Event>();
            var retryTimes = 5;
            configuration.RetryForTimes(5);
            Assert.AreEqual(retryTimes, configuration.MaxRetryTimes);
        }

        [Test]
        public void WhenConfiguringRetryForeverWithRetryTimesSetShouldThrowException()
        {
            var configuration = new RetryPolicyConfiguration<Event>();
            configuration.RetryForTimes(5);
            configuration.ForEachRetryWait(TimeSpan.FromSeconds(20));
            Assert.Throws<CantRetryForeverOverRetryTimesConfigurationException>(() => configuration.RetryForever());
        }

        [Test]
        public void WhenConfiguringRetryForeverWithWaitFailureShouldSetValueCorrectly()
        {
            var configuration = new RetryPolicyConfiguration<TestEvent>();
            var waitTimeInput1 = TimeSpan.FromSeconds(5);
            configuration.ForEachRetryWait(waitTimeInput1);

            var waitTimeOutput1 = configuration.RetryTime(new Failure<TestEvent>(new TestEvent(), 5,
                new Exception()));
            Assert.AreEqual(waitTimeOutput1.TotalMilliseconds, waitTimeInput1.TotalMilliseconds);


            var waitTimeInput2 = TimeSpan.FromSeconds(10);
            configuration.ForEachRetryWait(f => waitTimeInput2);

            var waitTimeOutput2 = configuration.RetryTime(new Failure<TestEvent>(new TestEvent(), 5,
                new Exception()));
            Assert.AreEqual(waitTimeOutput2.TotalMilliseconds, waitTimeInput2.TotalMilliseconds);
        }

        [Test]
        public void WhenConfiguringRetryForeverShouldSetPropertyCorrectly()
        {
            var configuration = new RetryPolicyConfiguration<Event>();
            configuration.RetryForever();
            Assert.IsTrue(configuration.ForeverRetry);
        }

        public class TestEvent : Event
        {

        }
    }
}