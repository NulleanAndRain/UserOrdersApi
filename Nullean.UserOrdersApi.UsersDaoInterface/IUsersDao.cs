using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.UsersDaoInterface
{
    public interface IUsersDao
    {
        public Task<Response<User>> GetUserByName(string username);
        public Task<Response> CreateUser(User user);
        public Task<Response<UserDetailed>> GetUserDetials(Guid Id);
    }
}
