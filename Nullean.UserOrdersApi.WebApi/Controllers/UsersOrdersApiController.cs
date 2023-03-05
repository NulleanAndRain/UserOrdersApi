using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nullean.UserOrdersApi.UsersLogicInterface;
using Nullean.UserOrdersApi.OrdersLogicInterface;
using Nullean.UserOrdersApi.SearchLogicInterface;
using Nullean.UserOrdersApi.Entities;
using Nullean.UserOrdersApi.WebApi.Models;
using System.Security.Claims;
using Nullean.UserOrdersApi.WebApi.Services;

namespace Nullean.UserOrdersApi.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersOrdersApiController : Controller
    {
        private readonly IUsersBll _users;
        private readonly IOrdersBll _orders;
        private readonly ISearchBll _search;
        private readonly IRabbitTest _mq;

        private const string USER_ID_FIELD = "user_id";

        public UsersOrdersApiController(IUsersBll users, IOrdersBll orders, ISearchBll search, IRabbitTest rabbitTest)
        {
            _users = users;
            _orders = orders;
            _search = search;
            _mq = rabbitTest;
        }

        [HttpGet("/GetUserInfo")]
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(StatusCodeResult), 404)]
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
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        public async Task<IActionResult> LogIn([FromBody]LogInModel model)
        {
            var res = await _users.LoginUser(model.Username, model.Password);

            var user = res.ResponseBody;

            if (res.Errors?.Any() ?? false )
            {
                return BadRequest(res.Errors);
            }
            else
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(USER_ID_FIELD, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                };
                var claimIdentity = new ClaimsIdentity(claims, "Cookie");
                var claimPrincipal = new ClaimsPrincipal(claimIdentity);
                await HttpContext.SignInAsync("Cookie", claimPrincipal);
                return Ok(MapUser(user));
            } 
        }

        [AllowAnonymous]
        [HttpPost("/SignUp")]
        [ProducesResponseType(typeof(UserModel), 200)]
        [ProducesResponseType(typeof(Error), 400)]
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
                return Ok(MapUser(res.ResponseBody));
            }
        }

        [HttpPost("/LogOut")]
        [ProducesResponseType(typeof(OkResult), 200)]
        [ProducesResponseType(typeof(StatusCodeResult), 404)]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync("Cookie");
            return Ok();
        }

        [HttpPost("/AddTestOrder")]
        [ProducesResponseType(typeof(OkResult), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(StatusCodeResult), 404)]
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

        /// <summary>
        /// Returns all products with matching name (including particulary match)
        /// </summary>
        /// <param name="searchString">Products name to find</param>
        /// <param name="sortByPriceMode">[Optional] sort by price: true for ascending, false for descending, no param for no sorting</param>
        [AllowAnonymous]
        [HttpGet("/SearchProducts")]
        [ProducesResponseType(typeof(IEnumerable<Product>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        public async Task<IActionResult> SearchProductsByName([FromQuery]string searchString, bool? sortByPriceMode)
        {
            var res = await _search.SearchProductsByName(searchString);

            if (res.Errors?.Any() ?? false)
            {
                return BadRequest(res.Errors);
            }
            else
            {
                var products = res.ResponseBody;
                if (sortByPriceMode.HasValue)
                {
                    if (sortByPriceMode.Value)
                    {
                        products = products.OrderBy(p => p.Price);
                    }
                    else
                    {
                        products = products.OrderByDescending(p => p.Price);
                    }
                }
                return Ok(products);
            }
        }

        [Authorize(Roles = Constants.RoleNames.Admin)]
        [HttpGet("/SearchUsers")]
        [ProducesResponseType(typeof(IEnumerable<UserModel>), 200)]
        [ProducesResponseType(typeof(Error), 400)]
        [ProducesResponseType(typeof(StatusCodeResult), 404)]
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

        [AllowAnonymous]
        [HttpPost("/SendAMessage")]
        [ProducesResponseType(typeof(OkResult), 200)]
        public async Task<IActionResult> SendAMessage([FromBody]Message msg)
        {
            var token = new CancellationTokenSource().Token;
            var result = await _mq.SendAMessage(msg.message, token);
            return Ok(result);
        }

        private UserModel MapUser(User user)
        {
            return new UserModel
            {
                Id = user.Id,
                Username = user.Username,
                Orders = user.Orders ?? new List<Order>(),
                OrdersCount = user.TotalOrdersCount,
                OrdersTotalPrice = user.TotalOrdersPrice,
            };
        }
    }

    public class Message
    {
        public string message { get; set; }
    }
}
