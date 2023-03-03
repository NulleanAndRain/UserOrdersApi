using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.SearchLogicInterface
{
    public interface IOrderBll
    {
        public Task<Response<IEnumerable<Order>>> GetUserOrders(Guid id);
        public Task<Response> AddOrder(Order order, Guid userId);
    }
}
