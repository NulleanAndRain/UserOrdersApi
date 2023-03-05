using Nullean.UserOrdersApi.Entities;
using Nullean.UserOrdersApi.OrdersDaoInterface;
using Nullean.UserOrdersApi.SearchLogicInterface;
using Nullean.UserOrdersApi.UsersDaoInterface;

namespace Nullean.UserOrdersApi.SearchLogic
{
    public class SearchBll : ISearchBll
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
            var productsResponse = new Response<IEnumerable<Product>>();
            try
            {
                var res = await _ordersDao.GetAllProducts();
                if (res.Errors?.Any() ?? false)
                {
                    productsResponse.Errors = res.Errors;
                }
                else
                {
                    productsResponse.ResponseBody = res.ResponseBody.Where(p => p.Name.ToLower().Contains(name.ToLower()));
                }
            }
            catch (Exception ex)
            {
                HandleExeption(productsResponse, ex);
            }
            return productsResponse;
        }

        public async Task<Response<IEnumerable<User>>> SearchUsersByName(string name)
        {

            var usersResponse = new Response<IEnumerable<User>>();
            try
            {
                var res = await _usersDao.GetAllUsers();
                if (res.Errors?.Any() ?? false)
                {
                    usersResponse.Errors = res.Errors;
                }
                else
                {
                    usersResponse.ResponseBody = res.ResponseBody
                        .Where(u => u.Username.ToLower().Contains(name.ToLower()))
                        .Select(CalculateUserModel);
                }
            }
            catch (Exception ex)
            {
                HandleExeption(usersResponse, ex);
            }
            return usersResponse;
        }

        private void HandleExeption(Response response, Exception ex)
        {
            var e = new Error
            {
                Message = ex.Message
            };
            if (response.Errors?.Any() ?? false)
            {
                response.Errors.Add(e);
            }
            else
            {
                response.Errors = new List<Error>
                    {
                        e
                    };
            }
        }

        private User CalculateUserModel(User user)
        {
            if (user.Orders != null)
            {
                foreach (var order in user.Orders)
                {
                    order.TotalCount = order.Products.Count();
                    order.TotalPrice = order.Products.Sum(p => p.Price);
                }
                user.TotalOrdersCount = user.Orders.Count();
                user.TotalOrdersPrice = user.Orders.Sum(o => o.TotalPrice);
            }
            else
            {
                user.TotalOrdersCount = 0;
                user.TotalOrdersPrice = 0;
            }
            return user;
        }
    }
}