namespace Nullean.UserOrdersApi.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public class UserDetailed : User
    {
        public IEnumerable<Order> Orders { get; set; }
        public decimal TotalOrdersPrice { get; set; }
        public int TotalOrdersCount { get; set; }
    }
}
