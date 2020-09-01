using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventBus.RabbitMQ.Extensions.DependencyInjection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RabbitExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddRabbitMqEventBus(hostContext.Configuration);
                    services.Replace(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(TimedLogger<>)));
                    services.AddHostedService<Worker>();
                });
    }

    public class TimedLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public TimedLogger(ILogger logger) => _logger = logger;

        public TimedLogger(ILoggerFactory loggerFactory) : this(new Logger<T>(loggerFactory)) { }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            _logger.Log(logLevel, eventId, state, exception, (s, ex) => $"[{DateTime.UtcNow:HH:mm:ss.fff}]: {formatter(s, ex)}");

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
    }
}
