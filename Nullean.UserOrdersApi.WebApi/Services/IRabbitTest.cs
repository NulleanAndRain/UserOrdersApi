namespace Nullean.UserOrdersApi.WebApi.Services
{
    public interface IRabbitTest
    {
        public Task<string> SendAMessage(string message, CancellationToken cancellationToken = default);
    }
}
