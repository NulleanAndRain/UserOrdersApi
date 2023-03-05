namespace Nullean.UserOrdersApi.Entities.Constants
{
    public class ConfigConstants
    {
        public const string SqlConnectionName = "DefaultConnection";
        public const string RabbitMqConnectionName = "RabbitMqQueueUri";

        public const string QueuesSectionName = "Queues";
        public const string UsersServiceQueue = "UsersServiceQueue";
        public const string OrdersServiceQueue = "OrdersServiceQueue";
        public const string SearchServiceQueue = "SearchServiceQueue";
    }
}
