using Microsoft.EntityFrameworkCore;
using Nullean.UserOrdersApi.OrdersDao;
using Nullean.UserOrdersApi.EFContext;
using Nullean.UserOrdersApi.Entities;

using OrderModel = Nullean.UserOrdersApi.Entities.Order;
using ProductModel = Nullean.UserOrdersApi.Entities.Product;

using OrderEF = Nullean.UserOrdersApi.EFContext.EfEntities.Order;
using ProductEF = Nullean.UserOrdersApi.EFContext.EfEntities.Product;

namespace Nullean.UserOrdersApi.OrdersDaoEF
{
    public class OrdersDaoEF : IOrdersDao
    {
        private readonly AppDbContext _ctx;

        public OrdersDaoEF(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Response> AddOrder(OrderModel order, Guid userId)
        {
            var response = new Response();
            try
            {
                var prToAdd = new List<ProductEF>();
                var o = _ctx.Orders.SingleOrDefault(o1 => o1.OrderId == order.Id);
                foreach (var p in order.Products)
                {
                    if (_ctx.Products.Any(p1 => p.Id == p1.ProductId))
                    {
                        var p1 = _ctx.Products.FirstOrDefault(p_t => p_t.ProductId == p.Id);
                        prToAdd.Add(p1);
                    }
                    else
                    {
                        var p_ef = new ProductEF
                        {
                            ProductId = p.Id,
                            Name = p.Name,
                            Price = p.Price
                        };
                        _ctx.Products.Add(p_ef);
                        _ctx.SaveChanges();
                        prToAdd.Add(p_ef);
                    }
                }

                var o_ef = new OrderEF
                {
                    OrderId = order.Id,
                    UserId = userId,
                    Products = prToAdd
                };
                _ctx.Orders.Add(o_ef);
                _ctx.SaveChanges();


                _ctx.Users.SingleOrDefault(u => u.UserId == userId)
                    !.Orders.Add(o_ef);

                await _ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.Errors = new List<Error>()
                {
                    new Error
                    {
                        Message = ex.Message,
                    }
                };
            }
            return response;
        }

        public async Task<Response<IEnumerable<Order>>> GetUserOrders(Guid id)
        {
            var response = new Response<IEnumerable<Order>>();
            try
            {
                var orders = _ctx.Orders
                    .Include(o => o.Products)
                    .Where(o => o.UserId == id)
                    .Select(o => new OrderModel
                    {
                        Id = o.OrderId,
                        Products = o.Products.Select(p => new ProductModel
                        {
                            Id = p.ProductId,
                            Name = p.Name,
                            Price = p.Price
                        }).ToList()
                    });
                response.ResponseBody = await orders.ToListAsync();
            }
            catch (Exception ex)
            {
                response.Errors = new List<Error>()
                {
                    new Error
                    {
                        Message = ex.Message,
                    }
                };
            }
            return response;
        }
    }
}
