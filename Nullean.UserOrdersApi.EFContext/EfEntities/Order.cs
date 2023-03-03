namespace Nullean.UserOrdersApi.EFContext.EfEntities
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
