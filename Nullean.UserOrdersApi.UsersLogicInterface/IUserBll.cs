using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.UsersLogicInterface
{
    public interface IUserBll
    {
        public Task<Response<User>> LoginUser(string username, string password);
        public Task<Response<User>> CreateUser(User user);
        public Task<Response<UserDetailed>> GetUserDetials(Guid id);
    }
}
