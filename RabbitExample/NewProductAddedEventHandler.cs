using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RabbitExample
{
    public class NewProductAddedEventHandler : INotificationHandler<NewProductAddedEvent>
    {
        private readonly ILogger<NewProductAddedEventHandler> _logger;

        public NewProductAddedEventHandler(ILogger<NewProductAddedEventHandler> logger)
            => _logger = logger;

        public async Task Handle(NewProductAddedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"there we go...");
            //throw new Exception("bad idea");
            Thread.Sleep(TimeSpan.FromMilliseconds(350));
        }
    }
}