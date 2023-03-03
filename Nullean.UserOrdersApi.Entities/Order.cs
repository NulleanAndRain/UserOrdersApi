namespace Nullean.UserOrdersApi.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
}
