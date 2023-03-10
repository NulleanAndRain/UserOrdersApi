using Nullean.UserOrdersApi.Entities.Constants;
using Nullean.UserOrdersApi.Entities.ServiceEntities;
using Nullean.UserOrdersApi.UsersLogicInterface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.Services.UsersService
{
    public class UsersService : BackgroundService, IDisposable
    {
        private IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public UsersService(IConfiguration config, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
            string response = null;

            using (var scope = _serviceProvider.CreateScope())
            {
                IUsersBll users = scope.ServiceProvider.GetRequiredService<IUsersBll>();
                var query = JsonConvert.DeserializeObject<RcpQuery>(content);

                switch (query.MethodName)
                {
                    case nameof(users.LoginUser):
                        {
                            var queryData = JsonConvert.DeserializeObject<LogInModel>(query.QueryJson);
                            var res = await users.LoginUser(queryData.Username, queryData.Password);
                            response = JsonConvert.SerializeObject(res);
                            break;
                        }
                    case nameof(users.CreateUser):
                        {
                            var queryData = JsonConvert.DeserializeObject<User>(query.QueryJson);
                            var res = await users.CreateUser(queryData);
                            response = JsonConvert.SerializeObject(res);
                            break;
                        }
                    case nameof(users.GetUserDetials):
                        {
                            var queryData = JsonConvert.DeserializeObject<Guid>(query.QueryJson);
                            var res = await users.GetUserDetials(queryData);
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
