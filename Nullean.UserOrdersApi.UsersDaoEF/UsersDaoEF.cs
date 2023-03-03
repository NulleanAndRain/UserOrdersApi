using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Nullean.UserOrdersApi.UsersDaoInterface;
using Nullean.UserOrdersApi.EFContext;
using Nullean.UserOrdersApi.Entities;
using Nullean.UserOrdersApi.EFContext.EfEntities;

namespace Nullean.UserOrdersApi.UsersDaoEF
{
    public class UsersDaoEF : IUsersDao
    {
        private readonly AppDbContext _ctx;

        public UsersDaoEF(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Response> CreateUser(User user)
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

        public async Task<Response<IEnumerable<User>>> GetAllUsers()
        {
            var response = new Response<IEnumerable<User>>();
            try
            {
                var users = _ctx.Users
                    .MapUsers();
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

        public async Task<Response<IEnumerable<User>>> GetUsersByName(string username)
        {
            var response = new Response<IEnumerable<User>>();
            try
            {
                var users = await _ctx.Users
                    .Where(u => u.Username.Contains(username))
                    .MapUsers()
                    .ToListAsync();
                response.ResponseBody = users;
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
        public async Task<Response<User>> GetUserByName(string username)
        {
            var response = new Response<User>();
            try
            {
                var users = await _ctx.Users
                    .Where(u => u.Username == username)
                    .MapUsers()
                    .FirstOrDefaultAsync();
                response.ResponseBody = users;
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

        public async Task<Response<User>> GetUserDetials(Guid Id)
        {
            var response = new Response<User>();
            try
            {
                var user = await _ctx.Users
                    .Where(u => u.UserId == Id)
                    .MapUsers()
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

    internal static class QueryHelper
    {
        public static IQueryable<User> MapUsers(this IQueryable<UserEF> users)
        {
            return users.Select(u => new User
            {
                Id = u.UserId,
                Orders = u.Orders.Select(o => new Order
                {
                    Id = o.OrderId,
                    Products = o.Products.Select(p => new Product
                    {
                        Id = p.ProductId,
                        Price = p.Price,
                        Name = p.Name
                    })
                }),
                Username = u.Username,
                Password = u.Password,
                Role = u.Role,
            });
        }
    }
}
