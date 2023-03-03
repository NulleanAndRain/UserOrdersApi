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
                    productsResponse.ResponseBody = res.ResponseBody.Where(p => p.Name.Contains(name));
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
                var res = await _usersDao.GetUsersByName(name);
                if (res.Errors?.Any() ?? false)
                {
                    usersResponse.Errors = res.Errors;
                }
                else
                {
                    usersResponse.ResponseBody = res.ResponseBody;
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
    }
}