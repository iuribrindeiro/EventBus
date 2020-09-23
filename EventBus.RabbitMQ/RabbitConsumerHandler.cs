using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ
{
    public sealed class RabbitConsumerHandler : IRabbitConsumerHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ILogger<RabbitConsumerHandler> _logger;
        private readonly RabbitMqEventBusOptions _options;

        public RabbitConsumerHandler(
            IServiceProvider serviceProvider, 
            ISubscriptionManager subscriptionManager, 
            ILogger<RabbitConsumerHandler> logger, IOptions<RabbitMqEventBusOptions> options)
        {
            _serviceProvider = serviceProvider;
            _subscriptionManager = subscriptionManager;
            _logger = logger;
            _options = options.Value;
        }

        public async Task HandleAsync(IModel consumerChannel, BasicDeliverEventArgs eventArgs)
        {
            using var scope = _serviceProvider.CreateScope();
            var eventId = eventArgs.BasicProperties.CorrelationId;
            var eventName = eventArgs.RoutingKey;
            _logger.LogInformation($"New {eventName} with id {eventId} arrived");
            using (_logger.BeginScope($"{eventId}:{eventName}"))
            {
                if (TryRetriveEventType(eventName, out Type eventType) && 
                    TryDeserializeEvent(eventArgs, eventType, out object @event))
                {
                    try
                    {
                        var mediator = scope.ServiceProvider.GetService<IMediator>();
                        await mediator.Publish(@event);
                        _logger.LogInformation($"Event successfully handled");
                        consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                        _logger.LogInformation($"Event removed from queue");
                    }
                    catch (Exception ex)
                    {
                        consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                        _logger.LogError(ex, "Failed to handle event");
                        HandleExceptionEvent(consumerChannel, eventArgs, eventName, @event, ex);
                    }
                }
                else
                {
                    _logger.LogInformation($"Removing unreadable event from queue");
                    consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    PublishToPermanentDeadLetter(consumerChannel, eventArgs);
                }
                _logger.LogInformation($"Finishing handling event");
            }
        }

        private void HandleExceptionEvent(IModel consumerChannel, BasicDeliverEventArgs eventArgs, string eventName,
            object @event, Exception ex)
        {
            var subscription = _subscriptionManager.FindSubscription(eventName);
            dynamic dynamicSubscription = subscription;
            dynamic retryConfiguration = dynamicSubscription.RetryPolicyConfiguration;
            var newAttemptCount = IncrementAttempt(eventArgs);
            var failure = CreateFailure(subscription, @event, newAttemptCount, ex);
            var shouldRetry = retryConfiguration.Retry(failure);
            var retryAttemptsExceeded =
                !retryConfiguration.ForeverRetry && newAttemptCount >= retryConfiguration.MaxRetryTimes; 

            if (shouldRetry && !retryAttemptsExceeded)
            {
                var retryWait = RetrieveRetryWaitingTime(subscription, failure);
                if (retryWait.TotalMilliseconds > 0)
                {
                    _logger.LogInformation($"Re-queueing event with delay");
                    PublishToWaitingDeadLetter(consumerChannel, eventArgs, retryWait, eventName);
                }
                else
                {
                    _logger.LogInformation($"Re-queueing event without delay");
                    consumerChannel.BasicPublish(_options.ExchangeName, eventName, mandatory: true,
                        body: eventArgs.Body, basicProperties: eventArgs.BasicProperties);
                }
            }
            else if (!retryConfiguration.DiscardEvent(failure))
                PublishToPermanentDeadLetter(consumerChannel, eventArgs);
        }

        private TimeSpan RetrieveRetryWaitingTime(dynamic retryFunc, dynamic failure)
        {
            _logger.LogInformation("Retrieving retry waiting time");
            TimeSpan retryWait;
            try
            {
               retryWait = retryFunc != null ? retryFunc(failure) : throw new Exception("Retry Wait cannot be null");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was a error calculating the waiting time. Default will be set");
                retryWait = TimeSpan.FromSeconds(5);
            }
            _logger.LogInformation($"The retry waiting will bee {retryWait.TotalMilliseconds} milliseconds");
            return retryWait;
        }

        private dynamic CreateFailure(ISubscription subscription, object @event, int attempts, Exception exception)
        {
            var eventTypeArgs = new[] {subscription.EventType};
            _logger.LogInformation("Creating failure object");
            var constructedEventType = typeof(Failure<>).MakeGenericType(eventTypeArgs);
            dynamic failure = Activator.CreateInstance(constructedEventType, @event, attempts, exception);
            return failure;
        }

        private void PublishToPermanentDeadLetter(IModel consumerChannel, BasicDeliverEventArgs eventArgs)
        {
            _logger.LogInformation("Declaring permanent deadletter queue");
            consumerChannel.QueueDeclare($"{_options.QueueName}.error", durable: true, exclusive: false, autoDelete: false);
            _logger.LogInformation("Publishin to permanent deadletter");
            consumerChannel.BasicPublish(string.Empty, $"{_options.QueueName}.error", body: eventArgs.Body,
                basicProperties: eventArgs.BasicProperties);
        }

        private void PublishToWaitingDeadLetter(IModel consumerChannel, BasicDeliverEventArgs eventArgs, TimeSpan retryWait, string eventName)
        {
            var deadLetterName = $"{_options.QueueName}_{retryWait.TotalSeconds}.error";
            DeclareWaitingDeadLetter(consumerChannel, retryWait, eventName, deadLetterName);
            _logger.LogInformation("Publishing to deadletter exchange");
            consumerChannel.BasicPublish(deadLetterName, eventName, mandatory: true,
                body: eventArgs.Body, basicProperties: eventArgs.BasicProperties);
        }

        private void DeclareWaitingDeadLetter(IModel consumerChannel, TimeSpan retryWait, string eventName, string deadLetterName)
        {
            _logger.LogInformation($"Declaring waiting queue deadletter");
            var totalSecondsQueue = Convert.ToInt32(retryWait.TotalSeconds + 30);
            consumerChannel.QueueDeclare(deadLetterName,
                arguments: new Dictionary<string, object>()
                {
                    {"x-dead-letter-exchange", _options.ExchangeName},
                    {"x-message-ttl", Convert.ToInt64(retryWait.TotalMilliseconds)},
                    {"x-expires", Convert.ToInt64(TimeSpan.FromSeconds(totalSecondsQueue).TotalMilliseconds)},
                }, durable: true, exclusive: false, autoDelete: false);
            _logger.LogInformation($"Declaring waiting exchange deadletter");
            consumerChannel.ExchangeDeclare(deadLetterName, type: "topic", autoDelete: true);
            _logger.LogInformation($"Binding waiting exchange deadletter to queue");
            consumerChannel.QueueBind(deadLetterName,
                deadLetterName, eventName);
        }

        private int IncrementAttempt(BasicDeliverEventArgs eventArgs)
        {
            _logger.LogInformation("Incrementing event attempt");
            eventArgs.BasicProperties.Headers ??= new Dictionary<string, object>();
            if (eventArgs.BasicProperties.Headers.TryGetValue("attempts", out object attempts))
            {
                _logger.LogInformation("Attempt is already defined, incrementing 1");
                attempts = (int) attempts + 1;
                eventArgs.BasicProperties.Headers["attempts"] = attempts;
            }
            else
            {
                _logger.LogInformation("Attempt is not defined yet, setting to first try");
                attempts = 1;
                eventArgs.BasicProperties.Headers.Add("attempts", attempts);
            }

            var newAttempt = (int)attempts;
            _logger.LogInformation($"New retry attempt count: {newAttempt}");
            return newAttempt;
        }

        private bool TryDeserializeEvent(BasicDeliverEventArgs eventArgs, Type eventType, out object @event)
        {
            _logger.LogInformation($"Trying to deserialize event");
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            try
            {
                @event = JsonConvert.DeserializeObject(message, eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to deserialize event with body: {message}");
                @event = null;
                return false;
            }

            _logger.LogInformation($"Event was deserialized with body: {message}");
            return true;
        }

        private bool TryRetriveEventType(string eventName, out Type eventType)
        {
            _logger.LogInformation($"Trying to find event subscription");
            var eventSubscription = _subscriptionManager.FindSubscription(eventName);
            eventType = eventSubscription?.EventType;

            var subscriptionFound = eventType != null;

            if (subscriptionFound)
                _logger.LogInformation($"Event subscription found for type: {eventType.FullName}");
            else
                _logger.LogError($"Event subscription was not found");

            return subscriptionFound;
        }
    }
}