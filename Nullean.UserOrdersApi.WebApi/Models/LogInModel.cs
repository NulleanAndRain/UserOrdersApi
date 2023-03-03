using System.ComponentModel.DataAnnotations;

namespace Nullean.UserOrdersApi.WebApi.Models
{
    public class LogInModel
    {
        [Required(ErrorMessage = "Please enter Username")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Please enter Password")]
        public string Password { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
