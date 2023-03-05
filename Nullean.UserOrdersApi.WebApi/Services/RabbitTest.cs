using Nullean.UserOrdersApi.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

namespace Nullean.UserOrdersApi.WebApi.Services
{
    public class RabbitTest : IRabbitTest, IDisposable
    {
        private IConfiguration _config;
        private string _replyTo;
        private IConnection _connection;
        private IModel _channel;

        private string _route;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();

        public RabbitTest(IConfiguration config)
        {
            _config = config;

            var factory = new ConnectionFactory() { HostName = _config.GetConnectionString(ConfigConstants.RabbitMqConnectionName) };
            _route = _config.GetSection(ConfigConstants.QueuesSectionName).GetValue<string>(ConfigConstants.UsersServiceQueue);

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _replyTo = _channel.QueueDeclare().QueueName;

            _channel.QueueDeclare(queue: _route,
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += Consumer_Received;

            _channel.BasicConsume(consumer, queue: _replyTo, autoAck: true);
        }

        public Task<string> SendAMessage(string message, CancellationToken cancellationToken = default)
        {
            var props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = _replyTo;

            var tcs = new TaskCompletionSource<string>();
            callbackMapper.TryAdd(correlationId, tcs);

            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: string.Empty,
                            routingKey: _route,
                            basicProperties: props,
                            body: body);

            cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out _));
            return tcs.Task;
        }

        public void Dispose()
        {
            _connection.Dispose();
            _channel.Dispose();
        }

        private void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            if (!callbackMapper.TryRemove(e.BasicProperties.CorrelationId, out var tcs))
            {
                return;
            }
            var response = Encoding.UTF8.GetString(e.Body.ToArray());
            _channel.QueuePurge(_replyTo);
            tcs.TrySetResult(response);
        }
    }
}
