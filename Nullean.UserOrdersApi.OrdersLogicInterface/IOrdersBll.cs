using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.OrdersLogicInterface
{
    public interface IOrdersBll
    {
        public Task<Response<IEnumerable<Order>>> GetUserOrders(Guid id);
        public Task<Response> AddOrder(Order order, Guid userId);
    }
}
