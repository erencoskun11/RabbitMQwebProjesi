using RabbitMQ.Client;
using RabbitMQWeb.ExcelCreate.Models;
using System.Text;
using System.Text.Json;

namespace RabbitMQWeb.ExcelCreate.Services
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _rabbitmqClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)

        {
            _rabbitmqClientService = rabbitMQClientService;
        }

        public void Publish(CreateExcelMessage productImageCreatedEvent)
        {
            var channel = _rabbitmqClientService.Connect();

            var bodyString = JsonSerializer.Serialize(productImageCreatedEvent);

            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName,
                routingKey: RabbitMQClientService.RoutingExcel, basicProperties:
                properties, body: bodyByte);
        }
    }
}
