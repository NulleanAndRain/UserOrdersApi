using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using Nullean.UserOrdersApi.UsersLogicInterface;
using Nullean.UserOrdersApi.OrdersLogicInterface;
using Nullean.UserOrdersApi.SearchLogicInterface;

using Nullean.UserOrdersApi.UsersLogic;
using Nullean.UserOrdersApi.OrdersLogic;
using Nullean.UserOrdersApi.SearchLogic;

using Nullean.UserOrdersApi.UsersDaoInterface;
using Nullean.UserOrdersApi.OrdersDaoInterface;
using Nullean.UserOrdersApi.UsersDaoEF;
using Nullean.UserOrdersApi.OrdersDaoEF;
using Nullean.UserOrdersApi.EFContext;

using Nullean.UserOrdersApi.Entities;
using ModelConstants = Nullean.UserOrdersApi.WebApi.Models.Constants;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(cfg =>
{
    cfg.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IOrdersDao, OrdersDaoEF>();
builder.Services.AddScoped<IUsersDao, UsersDaoEF>();

builder.Services.AddScoped<IUserBll, UsersBll>();
builder.Services.AddScoped<IOrdersBll, OrdersBll>();
builder.Services.AddScoped<ISearchBll, SearchBll>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddAuthentication("Cookie")
    .AddCookie("Cookie", cfg =>
    {
        cfg.LoginPath = "/Home/Login";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ModelConstants.RoleNames.User, builder =>
    {
        builder.RequireClaim(ClaimTypes.Role, ModelConstants.RoleNames.User);
    });

    options.AddPolicy(ModelConstants.RoleNames.Admin, builder =>
    {
        builder.RequireAssertion(x => 
            x.User.HasClaim(ClaimTypes.Role, ModelConstants.RoleNames.User) ||
            x.User.HasClaim(ClaimTypes.Role, ModelConstants.RoleNames.Admin)
        );
    });

});

builder.Services.AddMvc();
builder.Services.AddControllersWithViews();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = true;
    });
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(ep =>
{
    ep.MapDefaultControllerRoute();
});
//app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    using var ctx = scope.ServiceProvider.GetService<AppDbContext>();
    ctx.Database.EnsureDeleted();
    ctx.Database.EnsureCreated();

    AddEntries(
        scope.ServiceProvider.GetService<IUserBll>(),
        scope.ServiceProvider.GetService<IOrdersBll>()
    );
}

app.Run();


void AddEntries(IUserBll _users, IOrdersBll _orders)
{

    var admin = new User
    {
        Username = "admin",
        Password = "admin",
        Role = ModelConstants.RoleNames.Admin,
    };

    var p1 = new Product
    {
        Id = ModelConstants.TestGuids.product1,
        Name = "Cheese",
        Price = 250,
    };

    var p2 = new Product
    {
        Id = ModelConstants.TestGuids.product2,
        Name = "Bread",
        Price = 40,
    };

    var p3 = new Product
    {
        Id = ModelConstants.TestGuids.product3,
        Name = "Butter",
        Price = 100,
    };

    var p4 = new Product
    {
        Id = ModelConstants.TestGuids.product4,
        Name = "Beer",
        Price = 90,
    };

    var o = new Order
    {
        Products = new List<Product> { p1, p2, p3, p4 }
    };

    var res1 = _users.CreateUser(admin).Result;

    if (!res1.Errors?.Any() ?? true)
    {
        _orders.AddOrder(o, res1.ResponseBody!.Id);
    }
}