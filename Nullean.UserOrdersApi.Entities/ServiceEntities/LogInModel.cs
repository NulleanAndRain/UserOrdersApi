using System.ComponentModel.DataAnnotations;

namespace Nullean.UserOrdersApi.Entities.ServiceEntities
{
    public class LogInModel
    {
        [Required(ErrorMessage = "Please enter Username")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Please enter Password")]
        public string Password { get; set; }
    }
}
