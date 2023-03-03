namespace Nullean.UserOrdersApi.EFContext.EfEntities
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
