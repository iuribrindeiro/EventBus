using Microsoft.EntityFrameworkCore;

namespace RabbitMqEventLogEFExample
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Deposit> Deposits { get; set; }
    }
}