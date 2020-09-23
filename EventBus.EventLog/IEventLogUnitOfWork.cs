using System.Threading.Tasks;

namespace EventBus.EventLog
{
    public interface IEventLogUnitOfWork
    {
        Task SaveChangesAsync();
    }
}