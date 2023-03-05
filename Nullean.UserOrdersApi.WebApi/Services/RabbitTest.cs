using RabbitMQ.Client;
using System.Text;

namespace Nullean.UserOrdersApi.WebApi.Services
{
    public class RabbitTest : IRabbitTest
    {
        private IConfiguration _config;

        public RabbitTest(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAMessage(string message)
        {
            var factory = new ConnectionFactory() { HostName = _config.GetConnectionString("RabbitMqQueueUri") };
            var route = _config.GetSection("Queues").GetValue<string>("UsersServiceQueue");
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: route,
                               durable: false,
                               exclusive: false,
                               autoDelete: false,
                               arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                               routingKey: route,
                               basicProperties: null,
                               body: body);
            }
        }
    }
}
