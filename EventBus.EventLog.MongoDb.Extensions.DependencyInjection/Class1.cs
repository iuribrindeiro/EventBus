using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.EventLog.MongoDb.Extensions.DependencyInjection
{
    public static class Class1
    {
    }

    public static class EventBusEventLogMongoDbDependencyResolver
    {
        public static IServiceCollection AddEventLog<T>(this IServiceCollection services, IConfiguration configuration)
            where T : class, IMongoSessionProvider
        {
            EventLogSchemaMapper.RegisterEventLog();

            services.AddScoped<IMongoSessionProvider, T>();
            services.Configure<EventLogMongoDbOptions>(configuration.GetSection("EventBus:EventLog:MongoDb"));
            services.AddScoped<MongoDbContext>();
            services.AddScoped<IEventLogUnitOfWork>(sp => sp.GetService<MongoDbContext>());
            services.AddScoped<IEventLogService, MongoEventLogService>();
            services.AddScoped<IPersistentEventTransaction, MongoPersistentEventTransaction>();

            return services;
        }
    }
}
