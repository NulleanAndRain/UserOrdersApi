using Microsoft.EntityFrameworkCore;

namespace Nullean.UserOrdersApi.EFContext.EfEntities
{
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
