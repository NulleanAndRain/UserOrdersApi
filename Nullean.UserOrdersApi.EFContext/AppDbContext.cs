using Microsoft.EntityFrameworkCore;
using Nullean.UserOrdersApi.EFContext.EfEntities;

namespace Nullean.UserOrdersApi.EFContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UserEF> Users { get; set; }
        public DbSet<OrderEF> Orders { get; set; }
        public DbSet<ProductEF> Products { get; set; }
    }
}
