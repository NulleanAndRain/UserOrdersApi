using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.OrdersLogicInterface
{
    public interface ISearchLogic
    {
        public Task<Response<IEnumerable<User>>> SearchUsersByName(string name);
        public Task<Response<IEnumerable<Product>>> SearchProductsByName(string name);
    }
}