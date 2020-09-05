using EventBus.EventLog.EntityFrameworkCore;
using EventBus.EventLog.EntityFrameworkCore.Extensions.DependencyInjection;
using EventBus.RabbitMQ.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RabbitMqEventLogEFExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationContext>(e => e.UseSqlServer(Configuration.GetConnectionString("ApplicationDb")));

            services.AddRabbitMqEventBus(Configuration);
            services.AddEventLog<ApplicationContext>(e => e.UseSqlServer(Configuration.GetConnectionString("EventLogDb")));

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var eventLogDatabaseCreator = scope.ServiceProvider.GetService<IEventLogDatabaseCreator>();
                var applicationContext = scope.ServiceProvider.GetService<ApplicationContext>();
                eventLogDatabaseCreator.EnsureDatabaseCreated();
                applicationContext.Database.EnsureCreated();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
