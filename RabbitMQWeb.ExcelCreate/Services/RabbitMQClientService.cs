using RabbitMQ.Client;

namespace RabbitMQWeb.ExcelCreate.Services
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;

        public static string ExchangeName = "ExcelDirectExchange";
        public static string RoutingExcel = "excel-route-file";
        public static string QueueName = "queue-excel-file";

        private readonly ILogger<RabbitMQClientService> _logger;

        public RabbitMQClientService(ConnectionFactory connectionFactory,
            ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            _connection ??= _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: ExchangeName, type: "direct", durable: true, autoDelete: false);
            _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingExcel);

            _logger.LogInformation("RabbitMQ connection established.");

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
