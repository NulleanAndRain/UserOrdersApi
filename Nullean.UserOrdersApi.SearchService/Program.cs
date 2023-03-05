using Microsoft.EntityFrameworkCore;
using Nullean.UserOrdersApi.EFContext;
using Nullean.UserOrdersApi.Entities.Constants;
using Nullean.UserOrdersApi.OrdersDaoEF;
using Nullean.UserOrdersApi.OrdersDaoInterface;
using Nullean.UserOrdersApi.SearchLogic;
using Nullean.UserOrdersApi.SearchLogicInterface;
using Nullean.UserOrdersApi.Services.SearchService;
using Nullean.UserOrdersApi.UsersDaoEF;
using Nullean.UserOrdersApi.UsersDaoInterface;

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

builder.Services.AddScoped<ISearchBll, SearchBll>();
builder.Services.AddScoped<IOrdersDao, OrdersDaoEF>();
builder.Services.AddScoped<IUsersDao, UsersDaoEF>();
builder.Services.AddHostedService<SearchService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();
