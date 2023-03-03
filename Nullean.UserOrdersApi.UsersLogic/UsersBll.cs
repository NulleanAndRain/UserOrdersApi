using Nullean.UserOrdersApi.UsersLogicInterface;
using Nullean.UserOrdersApi.UsersDaoInterface;
using Nullean.UserOrdersApi.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Nullean.UserOrdersApi.UsersLogic
{
    public class UsersBll : IUserBll
    {
        private readonly IUsersDao _dao;

        private readonly string Ex_UserNotFound = "Пользователь не найден";
        private readonly string Ex_PassNotMatch = "Неверный пароль";

        public UsersBll(IUsersDao dao)
        {
            _dao = dao;
        }

        public async Task<Response<User>> CreateUser(User user)
        {
            var response = new Response<User>();
            try
            {
                user.Id = Guid.NewGuid();
                user.Password = GetPasswordHash(user.Password);

                var res = await _dao.CreateUser(user);
                if (res.Errors?.Any() ?? false)
                {
                    response.Errors = res.Errors;
                }
                else
                {
                    response.ResponseBody = user;
                }
            }
            catch (Exception ex)
            {
                HandleExeption(response, ex);
            }
            return response;
        }

        public async Task<Response<User>> GetUserDetials(Guid id)
        {
            var response = new Response<User>();
            try
            {
                var res = await _dao.GetUserDetials(id);
                if (res.Errors?.Any() ?? false)
                {
                    response.Errors = res.Errors;
                }
                else
                {
                    var user = res.ResponseBody;
                    if (user != null)
                    {
                        user.Orders = user.Orders.Select(
                            o =>
                            {
                                o.TotalCount = o.Products.Count();
                                o.TotalPrice = o.Products.Sum(p => p.Price);
                                return o;
                            }
                        );
                        user.TotalOrdersCount = user.Orders.Count();
                        user.TotalOrdersPrice = user.Orders.Sum(o => o.TotalPrice);
                        response.ResponseBody = user;
                    }
                    else
                    {
                        response.Errors = new List<Error>
                        {
                            new Error
                            {
                                Message = Ex_UserNotFound
                            }
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                HandleExeption(response, ex);
            }
            return response;
        }

        public async Task<Response<User>> LoginUser(string username, string password)
        {
            var response = new Response<User>();
            try
            {
                var res = await _dao.GetUserByName(username);

                if (res.Errors?.Any() ?? false)
                {
                    response.Errors = res.Errors;
                }
                else
                {
                    if (res.ResponseBody == null)
                    {
                        response.Errors = new List<Error>
                        {
                            new Error
                            {
                                Message = Ex_UserNotFound
                            }
                        };
                    }
                    else
                    {
                        if (res.ResponseBody.Password != GetPasswordHash(password))
                        {
                            response.Errors = new List<Error>
                            {
                                new Error
                                {
                                    Message = Ex_PassNotMatch
                                }
                            };
                        }
                        else
                        {
                            response.ResponseBody = res.ResponseBody;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleExeption(response, ex);
            }
            return response;
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

        private string GetPasswordHash(string pass)
        {
            var data = Encoding.ASCII.GetBytes(pass);
            data = SHA256.HashData(data);
            return Encoding.ASCII.GetString(data);
        }
    }
}
