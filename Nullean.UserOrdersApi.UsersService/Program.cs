using Microsoft.EntityFrameworkCore;
using Nullean.UserOrdersApi.EFContext;
using Nullean.UserOrdersApi.Entities;
using Nullean.UserOrdersApi.UsersDaoEF;
using Nullean.UserOrdersApi.UsersDaoInterface;
using Nullean.UserOrdersApi.UsersLogic;
using Nullean.UserOrdersApi.UsersLogicInterface;
using Nullean.UserOrdersApi.UsersService;

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

builder.Services.AddScoped<IUsersBll, UsersBll>();
builder.Services.AddScoped<IUsersDao, UsersDaoEF>();
builder.Services.AddHostedService<UsersService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();
