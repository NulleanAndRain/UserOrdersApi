using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.OrdersDaoInterface
{
    public interface IOrdersDao
    {
        public Task<Response<IEnumerable<Order>>> GetUserOrders(Guid id);
        public Task<Response> AddOrder(Order order, Guid userId);
        public Task<Response<IEnumerable<Product>>> GetAllProducts();
    }
}
