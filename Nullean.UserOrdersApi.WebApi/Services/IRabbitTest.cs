namespace Nullean.UserOrdersApi.WebApi.Services
{
    public interface IRabbitTest
    {
        public Task SendAMessage(string message);
    }
}
