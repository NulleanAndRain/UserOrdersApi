using Newtonsoft.Json;
using Nullean.UserOrdersApi.Entities;
using Nullean.UserOrdersApi.Entities.Constants;
using Nullean.UserOrdersApi.Entities.ServiceEntities;
using Nullean.UserOrdersApi.UsersLogicInterface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

namespace Nullean.UserOrdersApi.WebApi.Services
{
    public class UserServiceCaller : IUsersBll, IDisposable
    {
        private IConfiguration _config;
        private string _replyTo;
        private IConnection _connection;
        private IModel _channel;

        private string _route;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();

        public UserServiceCaller(IConfiguration config)
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

        public async Task<Response<User>> CreateUser(User user)
        {
            var token = new CancellationTokenSource().Token;
            var response = await SendQuery(nameof(IUsersBll.CreateUser), user, token);
            return JsonConvert.DeserializeObject<Response<User>>(response);
        }

        public async Task<Response<User>> GetUserDetials(Guid id)
        {
            var token = new CancellationTokenSource().Token;
            var response = await SendQuery(nameof(IUsersBll.GetUserDetials), id, token);
            return JsonConvert.DeserializeObject<Response<User>>(response);
        }

        public async Task<Response<User>> LoginUser(string username, string password)
        {
            var loginData = new LogInModel
            {
                Username = username,
                Password = password
            };
            var token = new CancellationTokenSource().Token;
            var response = await SendQuery(nameof(IUsersBll.LoginUser), loginData, token);
            return JsonConvert.DeserializeObject<Response<User>>(response);
        }

        public void Dispose()
        {
            _connection.Dispose();
            _channel.Dispose();
        }

        private Task<string> SendQuery(string methdod, object data, CancellationToken cancellationToken = default)
        {
            var props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = _replyTo;

            var tcs = new TaskCompletionSource<string>();
            callbackMapper.TryAdd(correlationId, tcs);

            var query = new RcpQuery
            {
                MethodName = methdod,
                QueryJson = JsonConvert.SerializeObject(data),
            };

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(query));

            _channel.BasicPublish(exchange: string.Empty,
                            routingKey: _route,
                            basicProperties: props,
                            body: body);

            cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out _));
            return tcs.Task;
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
