using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.UsersLogicInterface
{
    public interface IUsersBll
    {
        public Task<Response<User>> LoginUser(string username, string password);
        public Task<Response<User>> CreateUser(User user);
        public Task<Response<User>> GetUserDetials(Guid id);
    }
}
