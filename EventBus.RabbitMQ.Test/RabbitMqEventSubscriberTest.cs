using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using System;
using Microsoft.Extensions.Logging;
using NSubstitute.ReceivedExtensions;

namespace EventBus.RabbitMQ.Test
{
    public class RabbitMqEventSubscriberTest
    {
        private ISubscriptionManager _subscriptionManager;
        private IRabbitMqPersistentConnection _rabbitMqPersistentConnection;
        private RabbitMqEventBusOptions _rabbitMqEventBusOptionsValue;
        private RabbitMqEventSubscriber _sut;
        private IRabbitConsumerInitializer _rabbitConsumerInitializer;
        private IModel _model;
        private Subscription<Event> _subscription;

        [SetUp]
        public void SetUp()
        {
            _subscriptionManager = Substitute.For<ISubscriptionManager>();
            _rabbitMqPersistentConnection = Substitute.For<IRabbitMqPersistentConnection>();
            _rabbitConsumerInitializer = Substitute.For<IRabbitConsumerInitializer>();
            var rabbitMqEventBusOptions = Substitute.For<IOptions<RabbitMqEventBusOptions>>();
            var value = Substitute.For<RabbitMqEventBusOptions>();
            rabbitMqEventBusOptions.Value.Returns(value);
            _rabbitMqEventBusOptionsValue = value;
            var queueName = Guid.NewGuid().ToString();
            var exchangeName = Guid.NewGuid().ToString();
            value.QueueName.Returns(queueName);
            value.ExchangeName.Returns(exchangeName);
            _model = Substitute.For<IModel>();
            _rabbitMqPersistentConnection.CreateModel().Returns(_model);
            _subscription = new Subscription<Event>();
            _subscriptionManager.AddSubscription<Event>().Returns(_subscription);


            _sut = new RabbitMqEventSubscriber(_subscriptionManager, _rabbitMqPersistentConnection, _rabbitConsumerInitializer, rabbitMqEventBusOptions, Substitute.For<ILogger<RabbitMqEventSubscriber>>());

            _rabbitConsumerInitializer.Received(1).InitializeConsumersChannelsAsync();
        }

        [Test]
        public void WhenSubscribingToEventShouldSubscribeToSubscriptionManager()
        {
            _sut.Subscribe<Event>();
            
            _subscriptionManager.Received(1).AddSubscription<Event>();
        }

        [Test]
        public void WhenSubscribingToEventAndNotConnectedToRabbitShouldConnect()
        {
            _rabbitMqPersistentConnection.IsConnected.Returns(false);
            _sut.Subscribe<Event>();

            _rabbitMqPersistentConnection.Received(1).TryConnect();
        }

        [Test]
        public void WhenSubscribindToEventAndConnectedToRabbitShouldNotConnect()
        {
            _rabbitMqPersistentConnection.IsConnected.Returns(true);
            _sut.Subscribe<Event>();

            _rabbitMqPersistentConnection.DidNotReceive().TryConnect();
        }

        [Test]
        public void WhenSubscribingToEventShouldBindRabbitQueueToExchangeWithRoutingKey()
        {
            _sut.Subscribe<Event>();
            _model.Received(1).QueueBind(
                queue: _rabbitMqEventBusOptionsValue.QueueName, 
                exchange: _rabbitMqEventBusOptionsValue.ExchangeName, 
                routingKey: _subscription.EventName);
        }
    }
}
