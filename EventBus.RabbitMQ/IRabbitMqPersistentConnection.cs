using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMQ
{
    public interface IRabbitMqPersistentConnection : IDisposable
    {
        bool IsConnected { get; }
        void TryConnect();
        IModel CreateModel();
        EventingBasicConsumer CreateConsumer(IModel model);
    }
}