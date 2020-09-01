using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.EventLog.EntityFrameworkCore.Extensions.DependencyInjection
{
    public static class EventBusEventLogEntityFrameworkCoreDependencyResolver
    {
        public static IServiceCollection AddEventLog<T>(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> optionsAction = null) where T : DbContext
        {
            services.AddDbContext<EventLogDbContext>(optionsAction);
            services.AddScoped<IEventLogPublisher, EventLogPublisher>();
            services.AddScoped<IPersistentEventTransaction, PersistentEventTransaction>();
            services.AddScoped<IDbContextApplicationProvider>(
                sp => new DbContextApplicationProvider(sp.GetService<T>()));
            services.AddScoped<IEventLogDatabaseCreator>(sp => sp.GetService<EventLogDbContext>());
            return services;
        }
    }
}