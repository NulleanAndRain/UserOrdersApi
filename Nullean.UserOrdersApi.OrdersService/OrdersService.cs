using Nullean.UserOrdersApi.Entities.Constants;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Newtonsoft.Json;
using Nullean.UserOrdersApi.Entities.ServiceEntities;
using System.Text;
using Nullean.UserOrdersApi.OrdersLogicInterface;

namespace Nullean.UserOrdersApi.Services.OrdersService
{
    public class OrdersService : BackgroundService
    {
        private IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public OrdersService(IConfiguration config, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var factory = new ConnectionFactory { HostName = config.GetConnectionString(ConfigConstants.RabbitMqConnectionName) };
            var queue = config.GetSection(ConfigConstants.QueuesSectionName).GetValue<string>(ConfigConstants.OrdersServiceQueue);
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
                IOrdersBll orders = scope.ServiceProvider.GetRequiredService<IOrdersBll>();
                var query = JsonConvert.DeserializeObject<RcpQuery>(content);

                switch (query.MethodName)
                {
                    case nameof(orders.AddOrder):
                        {
                            var queryData = JsonConvert.DeserializeObject<AddOrderModel>(query.QueryJson);
                            var res = await orders.AddOrder(queryData.Order, queryData.UserId);
                            response = JsonConvert.SerializeObject(res);
                            break;
                        }
                    case nameof(orders.GetUserOrders):
                        {
                            var queryData = JsonConvert.DeserializeObject<Guid>(query.QueryJson);
                            var res = await orders.GetUserOrders(queryData);
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
