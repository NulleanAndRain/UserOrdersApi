using Nullean.UserOrdersApi.OrdersLogicInterface;
using Nullean.UserOrdersApi.OrdersDaoInterface;
using Nullean.UserOrdersApi.Entities;

namespace Nullean.UserOrdersApi.OrdersLogic
{
    public class OrdersBll : IOrdersBll
    {
        private readonly IOrdersDao _dao;

        public OrdersBll(IOrdersDao dao)
        {
            _dao = dao;
        }

        public async Task<Response> AddOrder(Order order, Guid userId)
        {
            var response = new Response();
            try
            {
                order.Id = Guid.NewGuid();
                var res = await _dao.AddOrder(order, userId);
                if (res.Errors?.Any() ?? false)
                {
                    response.Errors = res.Errors;
                }
            }
            catch (Exception ex)
            {
                var e = new Error
                {
                    Message = ex.Message
                };
                if (response.Errors?.Any() ?? false)
                {
                    response.Errors.Add(e);
                }
                else
                {
                    response.Errors = new List<Error>
                    {
                        e
                    };
                }
            }
            return response;
        }

        public async Task<Response<IEnumerable<Order>>> GetUserOrders(Guid id)
        {
            var response = new Response<IEnumerable<Order>>();
            try
            {
                var res = await _dao.GetUserOrders(id);
                if (res.Errors?.Any() ?? false)
                {
                    response.Errors = res.Errors;
                }
                else
                {
                    var orders = res.ResponseBody;
                    if (orders != null)
                    {
                        orders = orders.Select(o =>
                        {
                            o.TotalCount = o.Products.Count();
                            o.TotalPrice = o.Products.Sum(p => p.Price);
                            return o;
                        }).ToList();
                    }
                    response.ResponseBody = orders;
                }
            }
            catch (Exception ex)
            {
                var e = new Error
                {
                    Message = ex.Message
                };
                if (response.Errors?.Any() ?? false)
                {
                    response.Errors.Add(e);
                }
                else
                {
                    response.Errors = new List<Error>
                    {
                        e
                    };
                }
            }
            return response;
        }
    }
}
