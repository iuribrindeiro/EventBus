using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace EventBus.Test
{
    public class SubscriptionManagerTest
    {
        [Test]
        public void EnsureImAbleToAddAndRetrieveSubscriptions()
        {
            var subscriptionManager = new SubscriptionManager();
            var subscriptions = new List<ISubscription>()
            {
                subscriptionManager.AddSubscription<EventOne>(),
                subscriptionManager.AddSubscription<EventTwo>(),
                subscriptionManager.AddSubscription<EventThree>(),
                new Subscription<Event>()
            };

            Assert.IsTrue(subscriptions.Contains(subscriptionManager.FindSubscription<EventOne>()));
            Assert.IsTrue(subscriptions.Contains(subscriptionManager.FindSubscription<EventTwo>()));
            Assert.IsTrue(subscriptions.Contains(subscriptionManager.FindSubscription<EventThree>()));
            Assert.IsFalse(subscriptions.Contains(subscriptionManager.FindSubscription<Event>()));
        }

        public class EventOne : Event
        {
            public string ParticularPropertyOfEventOne { get; set; }
        }

        public class EventTwo : Event
        {

        }

        public class EventThree : Event
        {

        }
    }
}