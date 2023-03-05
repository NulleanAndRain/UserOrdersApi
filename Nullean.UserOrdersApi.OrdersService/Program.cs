using Microsoft.EntityFrameworkCore;
using Nullean.UserOrdersApi.EFContext;
using Nullean.UserOrdersApi.Entities.Constants;
using Nullean.UserOrdersApi.OrdersDaoEF;
using Nullean.UserOrdersApi.OrdersDaoInterface;
using Nullean.UserOrdersApi.OrdersLogic;
using Nullean.UserOrdersApi.OrdersLogicInterface;
using Nullean.UserOrdersApi.Services.OrdersService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(cfg =>
{
    cfg.UseSqlServer(builder.Configuration.GetConnectionString(ConfigConstants.SqlConnectionName));
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IOrdersBll, OrdersBll>();
builder.Services.AddScoped<IOrdersDao, OrdersDaoEF>();
builder.Services.AddHostedService<OrdersService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();
