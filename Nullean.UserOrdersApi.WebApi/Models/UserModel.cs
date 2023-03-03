using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.WebApi.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public IEnumerable<Order> Orders { get; set; }
        public int OrdersCount { get; set; }
        public decimal OrdersTotalPrice { get; set; }
    }
}
