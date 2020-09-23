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

        public Task PublishManyAsync(Event[] events) 
            => PublishManyAsync(events.Select(e => new EventPublishRequest(JsonConvert.SerializeObject(e), e.Id, e.Name)).ToArray());

        public async Task PublishManyAsync(EventPublishRequest[] publishRequests)
        {
            _logger.LogInformation($"Publishing {publishRequests.Length} events");
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            using var channel = _persistentConnection.CreateModel();
            var batchPublish = channel.CreateBasicPublishBatch();
            foreach (var publishRequest in publishRequests)
            {
                var props = channel.CreateBasicProperties();
                var eventName = publishRequest.EventName;
                _logger.LogInformation($"Adding event {eventName} with id: {publishRequest.EventId} to batch");
                props.CorrelationId = publishRequest.EventId.ToString();
                batchPublish
                    .Add(_options.ExchangeName,
                        routingKey: eventName,
                        mandatory: false,
                        properties: props,
                        body: Encoding.UTF8.GetBytes(publishRequest.EventBody));
            }

            batchPublish.Publish();
            _logger.LogInformation("All events were published");
        }
    }
}