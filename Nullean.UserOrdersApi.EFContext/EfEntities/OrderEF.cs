using Microsoft.EntityFrameworkCore;

namespace Nullean.UserOrdersApi.EFContext.EfEntities
{
    [PrimaryKey(nameof(OrderId))]
    public class OrderEF
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public UserEF User { get; set; }
        public ICollection<ProductEF> Products { get; set; }
    }
}
