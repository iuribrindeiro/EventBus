using System;

namespace RabbitMqEventLogEFExample
{
    public class Deposit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int ProductCount { get; set; }
    }
}