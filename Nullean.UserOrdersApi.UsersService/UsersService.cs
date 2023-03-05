using Nullean.UserOrdersApi.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Nullean.UserOrdersApi.UsersService
{
    public class UsersService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;

        public UsersService(IConfiguration config)
        {
            var factory = new ConnectionFactory { HostName = config.GetConnectionString(ConfigConstants.RabbitMqConnectionName) };
            var queue = config.GetSection(ConfigConstants.QueuesSectionName).GetValue<string>(ConfigConstants.UsersServiceQueue);
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += Consumer_Received;
            consumer.Received += async (_, _) =>
            {
                await Task.Delay(100);
                if (consumer.ShutdownReason != null)
                    Console.WriteLine($"shutdown reason: {consumer.ShutdownReason}");
            };
            _channel.BasicConsume(queue, autoAck: true, consumer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
        }

        public override void Dispose()
        {
            Console.WriteLine("Disposing");
            base.Dispose();
            _channel.Dispose();
            _connection.Dispose();
        }

        private void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            var props = e.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            var content = Encoding.UTF8.GetString(e.Body.ToArray());
            Console.WriteLine($"Получено сообщение: {content}");
            var response = string.Join("", content.Reverse().ToList());
            var responseBytes = Encoding.UTF8.GetBytes(response);

            _channel.BasicPublish(exchange: string.Empty, 
                routingKey: props.ReplyTo, 
                basicProperties: props, 
                body: responseBytes);
        }
    }
}
