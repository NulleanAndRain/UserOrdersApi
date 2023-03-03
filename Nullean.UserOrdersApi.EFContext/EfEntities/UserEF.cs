using Microsoft.EntityFrameworkCore;

namespace Nullean.UserOrdersApi.EFContext.EfEntities
{
    [Index(nameof(Username), IsUnique = true)]
    [PrimaryKey(nameof(UserId))]
    public class UserEF
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public ICollection<OrderEF> Orders { get; set; }
    }
}
