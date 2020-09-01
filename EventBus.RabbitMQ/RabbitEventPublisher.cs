using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("EventBus.RabbitMQ.Extensions.DependencyInjection")]
namespace EventBus.RabbitMQ
{
    public sealed class RabbitEventPublisher : IEventPublisher
    {
        private readonly IRabbitMqPersistentConnection _persistentConnection;
        private readonly ILogger<RabbitEventPublisher> _logger;
        private readonly RabbitMqEventBusOptions _options;

        public RabbitEventPublisher(
            IOptions<RabbitMqEventBusOptions> options, 
            IRabbitMqPersistentConnection persistentConnection, 
            ILogger<RabbitEventPublisher> logger)
        {
            _persistentConnection = persistentConnection;
            _logger = logger;
            _options = options.Value;
        }

        public async Task PublishAsync<T>(T @event) where T : Event
        {
            var eventName = @event.GetType().Name;
            _logger.LogInformation($"Publishing {eventName} with id: {@event.Id}");
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            using var channel = _persistentConnection.CreateModel();
            var props = channel.CreateBasicProperties();
            props.CorrelationId = @event.Id.ToString();
            channel.BasicPublish(
                _options.ExchangeName, 
                routingKey: eventName, 
                basicProperties: props, 
                body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)));
            _logger.LogInformation($"Event published");
        }

        public async Task PublishManyAsync(Event[] events)
        {
            _logger.LogInformation($"Publishing {events.Count()} events");
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            using var channel = _persistentConnection.CreateModel();
            var batchPublish = channel.CreateBasicPublishBatch();
            foreach (var @event in events)
            {
                var props = channel.CreateBasicProperties();
                var eventName = @event.GetType().Name;
                _logger.LogInformation($"Adding event {eventName} with id: {@event.Id} to batch");
                props.CorrelationId = @event.Id.ToString();
                batchPublish
                    .Add(_options.ExchangeName, 
                        routingKey: eventName, 
                        mandatory: false, 
                        properties: props, 
                        body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)));
            }

            batchPublish.Publish();
            _logger.LogInformation("All events were published");
        }
    }
}