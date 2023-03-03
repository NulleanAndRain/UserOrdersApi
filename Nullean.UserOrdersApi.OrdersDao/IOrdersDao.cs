using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.OrdersDao
{
    public interface IOrdersDao
    {
        public Task<Response<IEnumerable<Order>>> GetUserOrders(Guid id);
        public Task<Response> AddOrder(Order order, Guid userId);
    }
}
