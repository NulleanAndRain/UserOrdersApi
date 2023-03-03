namespace Nullean.UserOrdersApi.EFContext.EfEntities
{
    public class OrderEF
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public UserEF User { get; set; }
        public ICollection<ProductEF> Products { get; set; }
    }
}
