using NUnit.Framework;

namespace EventBus.Test
{
    public class SubscriptionTest
    {
        [Test]
        public void WhenCreatingASubscriptionEventTypeShouldBeSetCorrectly()
        {
            var subscription = new Subscription<Event>();
            Assert.AreEqual(typeof(Event), subscription.EventType);
        }

        [Test]
        public void EventNameShouldReturnTheEventTypeName()
        {
            var subscription = new Subscription<Event>();
            Assert.AreEqual(typeof(Event).Name, subscription.EventName);
        }


        [Test]
        public void WhenCreatingASubscriptioAConfigurationWithRetryForeverAndNoWaitTimeShouldBeCreated()
        {
            var subscription = new Subscription<Event>();
            Assert.IsTrue(subscription.RetryPolicyConfiguration.ForeverRetry);
            Assert.IsNull(subscription.RetryPolicyConfiguration.RetryTime);
        }

        [Test]
        public void WhenConfiguringASubscriptionForRetriesShouldUseTheSameRetryConfigurationOfSubscrption()
        {
            var subscription = new Subscription<Event>();
            var configuration = subscription.RetryPolicyConfiguration;
            subscription.OnFailure(r => Assert.AreSame(r, configuration));
        }
    }
}
