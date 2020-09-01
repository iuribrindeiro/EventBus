using System.Threading.Tasks;

namespace EventBus.EventLog.EntityFrameworkCore
{
    public interface IEventLogDatabaseCreator
    {
        void EnsureDatabaseCreated();
    }
}