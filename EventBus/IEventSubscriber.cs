using System.Threading.Tasks;

namespace EventBus
{
    public interface IEventSubscriber
    {
        Subscription<T> Subscribe<T>() where T : Event;
        Task StartListeningAsync();
    }
}