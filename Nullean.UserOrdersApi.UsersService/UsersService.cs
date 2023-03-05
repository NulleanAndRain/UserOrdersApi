using Nullean.UserOrdersApi.UsersLogicInterface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

namespace Nullean.UserOrdersApi.UsersService
{
    public class UsersService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;

        public UsersService(IConfiguration config)
        {
            var factory = new ConnectionFactory { HostName = config.GetConnectionString("RabbitMqQueueUri") };
            var queue = config.GetSection("Queues").GetValue<string>("UsersServiceQueue");
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += Consumer_Received;
            _channel.BasicConsume(queue, autoAck: true, consumer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += Consumer_Received;
        }

        public override void Dispose()
        {
            base.Dispose();
            _channel.Dispose();
            _connection.Dispose();
        }

        private void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            var content = Encoding.UTF8.GetString(e.Body.ToArray());
            
            Console.WriteLine($"Получено сообщение: {content}");
        }
    }
}
