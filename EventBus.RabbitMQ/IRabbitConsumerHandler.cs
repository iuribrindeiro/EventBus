using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMQ
{
    public interface IRabbitConsumerHandler
    {
        Task HandleAsync(IModel consumerChannel, BasicDeliverEventArgs eventArgs);
    }
}