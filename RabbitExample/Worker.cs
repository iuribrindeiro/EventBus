using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RabbitExample
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly IServiceProvider _serviceProvider;
        private bool _eventsSubscribed;

        public Worker(ILogger<Worker> logger, IEventSubscriber eventSubscriber, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _eventSubscriber = eventSubscriber;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_eventsSubscribed)
                {
                    _eventSubscriber
                        .Subscribe<NewProductAddedEvent>()
                        .OnFailure(cf=>
                        {
                            cf.RetryForTimes(5)
                                .ForEachRetryWait(TimeSpan.FromSeconds(5))
                                .ShouldRetry()
                                .ShouldDiscardEventAfterFailures();
                        });

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        await scope.ServiceProvider.GetService<IEventPublisher>().PublishAsync(new NewProductAddedEvent());   
                    }
                    
                    await _eventSubscriber.StartListeningAsync();
                    _eventsSubscribed = true;
                }
            }
        }
    }

    public class NewProductAddedEvent : Event
    {
        public bool DomainProp { get; set; }
    }
}
