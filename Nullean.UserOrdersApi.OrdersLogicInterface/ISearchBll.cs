using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.OrdersLogicInterface
{
    public interface ISearchBll
    {
        public Task<Response<List<User>>> SearchUsersByName(string name);
        public Task<Response<List<Product>>> SearchProductsByName(string name);
    }
}