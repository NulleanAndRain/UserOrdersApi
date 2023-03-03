using Nullean.UserOrdersApi.Entities;
using Nullean.UserOrdersApi.OrdersDaoInterface;
using Nullean.UserOrdersApi.OrdersLogicInterface;
using Nullean.UserOrdersApi.UsersDaoInterface;

namespace Nullean.UserOrdersApi.SearchLogic
{
    public class SearchBll : ISearchLogic
    {
        private IUsersDao _usersDao;
        private IOrdersDao _ordersDao;

        public SearchBll(IUsersDao usersDao, IOrdersDao ordersDao)
        {
            _usersDao = usersDao;
            _ordersDao = ordersDao;
        }

        public async Task<Response<IEnumerable<Product>>> SearchProductsByName(string name)
        {
            var productsResponse = await _ordersDao.GetAllProducts();
            IEnumerable<Product> products = null;
            if (productsResponse.Errors?.Count == 0)
            {
                products = productsResponse.ResponseBody.Where(p => p.Name.Contains(name));
            }
            return new Response<IEnumerable<Product>>()
            {
                ResponseBody = products,
                Errors = productsResponse.Errors
            };
        }

        public async Task<Response<IEnumerable<User>>> SearchUsersByName(string name)
        {
            var usersResponse = await _usersDao.GetAllUsers();
            IEnumerable<User> users = null;
            if (usersResponse.Errors?.Count == 0)
            {
                users = usersResponse.ResponseBody.Where(u => u.Username.Contains(name));
            }
            return new Response<IEnumerable<User>>()
            {
                ResponseBody = users,
                Errors = usersResponse.Errors
            };
        }
    }
}