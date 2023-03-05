using Newtonsoft.Json;
using Nullean.UserOrdersApi.Entities.Constants;
using Nullean.UserOrdersApi.Entities.ServiceEntities;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Nullean.UserOrdersApi.SearchLogicInterface;

namespace Nullean.UserOrdersApi.Services.SearchService
{
    public class SearchService : BackgroundService, IDisposable
    {
        private IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public SearchService(IConfiguration config, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var factory = new ConnectionFactory { HostName = config.GetConnectionString(ConfigConstants.RabbitMqConnectionName) };
            var queue = config.GetSection(ConfigConstants.QueuesSectionName).GetValue<string>(ConfigConstants.SearchServiceQueue);
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

        public override void Dispose()
        {
            base.Dispose();
            _channel.Dispose();
            _connection.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
        }

        private async void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            var props = e.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            var content = Encoding.UTF8.GetString(e.Body.ToArray());
            string response = string.Empty;

            using (var scope = _serviceProvider.CreateScope())
            {
                ISearchBll orders = scope.ServiceProvider.GetRequiredService<ISearchBll>();
                var query = JsonConvert.DeserializeObject<RcpQuery>(content);

                switch (query.MethodName)
                {
                    case nameof(orders.SearchUsersByName):
                        {
                            var queryData = JsonConvert.DeserializeObject<string>(query.QueryJson);
                            var res = await orders.SearchUsersByName(queryData);
                            response = JsonConvert.SerializeObject(res);
                            break;
                        }
                    case nameof(orders.SearchProductsByName):
                        {
                            var queryData = JsonConvert.DeserializeObject<string>(query.QueryJson);
                            var res = await orders.SearchProductsByName(queryData);
                            response = JsonConvert.SerializeObject(res);
                            break;
                        }
                    default: { break; }
                }
            }

            var responseBytes = Encoding.UTF8.GetBytes(response);

            _channel.BasicPublish(exchange: string.Empty,
                routingKey: props.ReplyTo,
                basicProperties: props,
                body: responseBytes);
        }
    }
}
