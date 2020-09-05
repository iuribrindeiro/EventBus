using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMQ.Test
{
    public class RabbitMqConsumerInitializerTest
    {
        private IRabbitConsumerHandler _rabbitConsumerHandler;
        private RabbitMqEventBusOptions _rabbitMqEventBusOptions;
        private IRabbitMqPersistentConnection _rabbitMqPersistentConnection;
        private RabbitConsumerInitializer _sut;
        private IModel _model;
        private EventingBasicConsumer _consumer;

        [SetUp]
        public void SetUp()
        {
            _rabbitMqPersistentConnection = Substitute.For<IRabbitMqPersistentConnection>();
            _rabbitConsumerHandler = Substitute.For<IRabbitConsumerHandler>();
            var rabbitMqEventBusOptions = Substitute.For<IOptions<RabbitMqEventBusOptions>>();
            var value = Substitute.For<RabbitMqEventBusOptions>();
            rabbitMqEventBusOptions.Value.Returns(value);
            _rabbitMqEventBusOptions = value;
            var queueName = Guid.NewGuid().ToString();
            var exchangeName = Guid.NewGuid().ToString();
            value.QueueName.Returns(queueName);
            value.ExchangeName.Returns(exchangeName);
            _model = Substitute.For<IModel>();
            _rabbitMqPersistentConnection.CreateModel().Returns(_model);
            _consumer = Substitute.For<EventingBasicConsumer>(Substitute.For<IModel>());
            _rabbitMqPersistentConnection.CreateConsumer(_model).Returns(_consumer);

            _sut = new RabbitConsumerInitializer(_rabbitMqPersistentConnection, rabbitMqEventBusOptions, _rabbitConsumerHandler, Substitute.For<ILogger<RabbitConsumerInitializer>>());
        }

        [Test]
        public async Task WhenInitializingConsumerChannelShouldTryConnectIfNotConnectedToRabbit()
        {
            _rabbitMqPersistentConnection.IsConnected.Returns(false);
            await _sut.InitializeConsumersChannelsAsync();
            _rabbitMqPersistentConnection.Received(1).TryConnect();
        }

        [Test]
        public async Task WhenInitializingConsumerChannelShouldNotTryConnectIfAlreadyConnectedToRabbit()
        {
            _rabbitMqPersistentConnection.IsConnected.Returns(true);
            await _sut.InitializeConsumersChannelsAsync();
            _rabbitMqPersistentConnection.DidNotReceive().TryConnect();
        }

        [Test]
        public async Task WhenInitializingConsumerChannelEnsureThatQueueAndExchangeAreCreatedBeforeConsumerStarts()
        {
            var queueName = _rabbitMqEventBusOptions.QueueName;
            var exchangeName = _rabbitMqEventBusOptions.ExchangeName;
            await _sut.InitializeConsumersChannelsAsync();
            Received.InOrder(() =>
            {
                _model.ExchangeDeclare(exchange: exchangeName, type: "topic");
                _model.QueueDeclare(queueName);
                _model.BasicConsume(queue: queueName, autoAck: false, _consumer);
            });
        }
    }
}