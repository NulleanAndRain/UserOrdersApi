using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.SearchLogicInterface
{
    public interface ISearchBll
    {
        public Task<Response<IEnumerable<User>>> SearchUsersByName(string name);
        public Task<Response<IEnumerable<Product>>> SearchProductsByName(string name);
    }
}