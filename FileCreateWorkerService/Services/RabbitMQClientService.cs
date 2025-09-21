using System;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FileCreateWorkerService.Services
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory _factory;
        private readonly ILogger<RabbitMQClientService> _logger;
        private IConnection? _connection;
        private IModel? _channel;

        // Queue adını uygulamanda kullandığın isimle eşleştir.
        public const string QueueName = "excel-file-create-queue";

        public RabbitMQClientService(ConnectionFactory factory, ILogger<RabbitMQClientService> logger)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger;
        }

        // Connect -> IModel döndüren basit bir helper
        public IModel Connect()
        {
            if (_channel != null && _channel.IsOpen)
            {
                return _channel;
            }

            _connection ??= _factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Queue declare (durable etc. ihtiyaçlarına göre ayarla)
            _channel.QueueDeclare(queue: QueueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            _logger.LogInformation("RabbitMQ connection and channel established. Queue: {Queue}", QueueName);
            return _channel;
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while disposing RabbitMQ client");
            }
        }
    }
}

