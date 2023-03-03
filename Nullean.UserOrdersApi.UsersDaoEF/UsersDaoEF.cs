using Nullean.UserOrdersApi.UsersDaoInterface;
using Nullean.UserOrdersApi.EFContext;
using Nullean.UserOrdersApi.Entities;
using Microsoft.EntityFrameworkCore;

using UserModel = Nullean.UserOrdersApi.Entities.User;
using OrderModel = Nullean.UserOrdersApi.Entities.Order;
using ProductModel = Nullean.UserOrdersApi.Entities.Product;

using UserEF = Nullean.UserOrdersApi.EFContext.EfEntities.User;

namespace Nullean.UserOrdersApi.UsersDaoEF
{
    public class UsersDaoEF : IUsersDao
    {
        private readonly AppDbContext _ctx;

        public UsersDaoEF(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Response> CreateUser(UserModel user)
        {
            var response = new Response();
            try
            {
                var u = new UserEF
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Password = user.Password,
                    Role = user.Role
                };
                await _ctx.AddAsync(u);
                await _ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.Errors = new List<Error>()
                {
                    new Error
                    {
                        Message = ex.Message
                    }
                };
            }
            return response;
        }

        public async Task<Response<IEnumerable<UserModel>>> GetAllUsers()
        {
            var response = new Response<IEnumerable<UserModel>>();
            try
            {
                var users = _ctx.Users
                    .Select(u => new UserModel
                    {
                        Id = u.UserId,
                        Username = u.Username,
                        Role = u.Role,
                        Password = u.Password,
                    });
                response.ResponseBody = await users.ToListAsync();
            }
            catch (Exception ex)
            {
                response.Errors = new List<Error>()
                {
                    new Error
                    {
                        Message = ex.Message,
                    }
                };
            }
            return response;
        }

        public async Task<Response<UserModel>> GetUserByName(string username)
        {
            var response = new Response<UserModel>();
            try
            {
                var user = await _ctx.Users
                    .Where(u => u.Username == username)
                    .Select(u => new UserModel
                    {
                        Id = u.UserId,
                        Username = u.Username,
                        Password = u.Password,
                        Role = u.Role
                    })
                    .FirstOrDefaultAsync();
                response.ResponseBody = user;
            }
            catch (Exception ex)
            {
                response.Errors = new List<Error>()
                {
                    new Error
                    {
                        Message = ex.Message
                    }
                };
            }
            return response;
        }

        public async Task<Response<UserDetailed>> GetUserDetials(Guid Id)
        {
            var response = new Response<UserDetailed>();
            try
            {
                var user = await _ctx.Users
                    //.Include(u => u.Orders)
                    //.Include(u => u.Orders.Select(o => o.Products))
                    .Where(u => u.UserId == Id)
                    .Select(u => new UserDetailed
                    {
                        Id = u.UserId,
                        Orders = u.Orders.Select(o => new OrderModel
                        {
                            Id = o.OrderId,
                            Products = o.Products.Select(p => new ProductModel
                            {
                                Id = p.ProductId,
                                Price = p.Price,
                                Name = p.Name
                            })
                        }),
                        Username = u.Username,
                        Password = u.Password
                    })
                    .SingleOrDefaultAsync();
                response.ResponseBody = user;
            }
            catch (Exception ex)
            {
                response.Errors = new List<Error>()
                {
                    new Error
                    {
                        Message = ex.Message
                    }
                };
            }
            return response;
        }
    }
}
