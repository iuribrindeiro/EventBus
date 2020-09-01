using System;

namespace RabbitMqEventLogEFExample
{
    public class Product
    {
        public string Name { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}