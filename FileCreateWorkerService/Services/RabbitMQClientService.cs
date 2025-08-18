using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCreateWorkerService.Services
{
     public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory _connectonFactory;
        private IConnection _connection;
        private IModel _channel;

        public static string QueueName = "queue-wxcel-file";

        private readonly ILogger<RabbitMQClientService> _logger;

        public RabbitMQClientService(ConnectionFactory connectionFactory,
            ILogger<RabbitMQClientService> logger)
        {
            _connectonFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {
            _connection = _connectonFactory.CreateConnection();

            if(_channel is { IsOpen: true})
            {
                return _channel;
            }
            _channel = _connection.CreateModel();
            _logger.LogInformation("connection is open with rabbitmq...");
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

                _logger.LogInformation("RabbitMQ connection closed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while closing RabbitMQ connection.");
            }
        }


    }
}
