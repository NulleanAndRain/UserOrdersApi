namespace Nullean.UserOrdersApi.EFContext.EfEntities
{
    public class ProductEF
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public ICollection<OrderEF> Orders { get; set; }
    }
}
