namespace Nullean.UserOrdersApi.Entities.ServiceEntities
{
    public class AddOrderModel
    {
        public Guid UserId { get; set; }
        public Order Order { get; set; }
    }
}
