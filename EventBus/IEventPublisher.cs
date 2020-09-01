using System.Threading.Tasks;

namespace EventBus
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event) where T : Event;
        Task PublishManyAsync(Event[] @events);
    }
} 
