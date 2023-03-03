using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nullean.UserOrdersApi.UsersLogicInterface;
using Nullean.UserOrdersApi.OrdersLogicInterface;
using Nullean.UserOrdersApi.SearchLogicInterface;
using Nullean.UserOrdersApi.Entities;
using Nullean.UserOrdersApi.WebApi.Models;
using System.Security.Claims;

namespace Nullean.UserOrdersApi.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersOrdersApiController : Controller
    {
        private readonly IUserBll _users;
        private readonly IOrdersBll _orders;
        private readonly ISearchBll _search;

        private const string USER_ID_FIELD = "user_id";

        public UsersOrdersApiController(IUserBll users, IOrdersBll orders, ISearchBll search)
        {
            _users = users;
            _orders = orders;
            _search = search;
        }

        [HttpGet("/GetUserInfo")]
        [Authorize(Policy = Constants.RoleNames.User)]
        public async Task<IActionResult> GetUserInfo([FromQuery] Guid id)
        {
            var res = await _users.GetUserDetials(id);

            if (res.Errors?.Any() ?? false)
            {
                await LogOut();
            }

            if (res.Errors?.Any() ?? false)
            {
                return BadRequest(res.Errors);
            }
            else
            {
                var user = res.ResponseBody;
                return Ok(MapUser(user));
            }
        }

        [AllowAnonymous]
        [HttpPost("/LogIn")]
        public async Task<IActionResult> LogIn([FromBody]LogInModel model)
        {
            var res = await _users.LoginUser(model.Username, model.Password);

            var user = res.ResponseBody;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(USER_ID_FIELD, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var claimIdentity = new ClaimsIdentity(claims, "Cookie");
            var claimPrincipal = new ClaimsPrincipal(claimIdentity);
            await HttpContext.SignInAsync("Cookie", claimPrincipal);

            if (res.Errors?.Any() ?? false)
            {
                return BadRequest(res.Errors);
            }
            else
            {
                return Ok(MapUser(user));
            } 
        }

        [AllowAnonymous]
        [HttpPost("/SignUp")]
        public async Task<IActionResult> SignUp([FromBody]SignUpModel model)
        {
            var res = await _users.CreateUser(new User
            {
                Username = model.Username,
                Password = model.Password,
                Role = Constants.RoleNames.User,
            });

            if (res.Errors?.Any() ?? false)
            {
                return BadRequest(res.Errors);
            }
            else
            {
                return Ok(MapUser(res.ResponseBody));
            }
        }

        [HttpPost("/LogOut")]
        public async Task LogOut()
        {
            await HttpContext.SignOutAsync("Cookie");
        }

        [HttpPost("/AddTestOrder")]
        public async Task<IActionResult> AddTestOrder()
        {
            var userId = Guid.Parse(User.Claims.SingleOrDefault(cl => cl.Type == USER_ID_FIELD).Value);

            var p1 = new Product
            {
                Id = Constants.TestGuids.product1,
                Name = "Cheese",
                Price = 250,
            };

            var p2 = new Product
            {
                Id = Constants.TestGuids.product2,
                Name = "Bread",
                Price = 40,
            };

            var p3 = new Product
            {
                Id = Constants.TestGuids.product3,
                Name = "Butter",
                Price = 100,
            };

            var o = new Order
            {
                Products = new List<Product> { p1, p2, p3 }
            };

            var res =  await _orders.AddOrder(o, userId);

            if (res.Errors?.Any() ?? false)
            {
                return BadRequest(res.Errors);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("/SearchProduct")]
        public async Task<IActionResult> SearchProductByName([FromQuery]string searchString)
        {
            var res = await _search.SearchProductsByName(searchString);

            if (res.Errors?.Any() ?? false)
            {
                return BadRequest(res.Errors);
            }
            else
            {
                return Ok(res.ResponseBody);
            }
        }

        [Authorize(Policy = Constants.RoleNames.Admin)]
        [HttpGet("/SearchUser")]
        public async Task<IActionResult> SearchUsersByName([FromQuery]string searchString)
        {
            var res = await _search.SearchUsersByName(searchString);
            if (res.ResponseBody != null)
            {
                var model = res.ResponseBody.Select(MapUser);
                return Ok(model);
            }
            else
            {
                return BadRequest(res.Errors);
            }
        }

        private UserModel MapUser(User user)
        {
            return new UserModel
            {
                Id = user.Id,
                Username = user.Username,
                Orders = user.Orders,
                OrdersCount = user.TotalOrdersCount,
                OrdersTotalPrice = user.TotalOrdersPrice,
            };
        }
    }
}
