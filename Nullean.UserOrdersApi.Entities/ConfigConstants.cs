namespace Nullean.UserOrdersApi.Entities
{
    public class ConfigConstants
    {
        public const string SqlConnectionName = "DefaultConnection";
        public const string RabbitMqConnectionName = "RabbitMqQueueUri";

        public const string QueuesSectionName = "Queues";
        public const string UsersServiceQueue = "UsersServiceQueue";
    }
}
