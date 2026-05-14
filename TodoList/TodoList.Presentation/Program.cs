using Microsoft.EntityFrameworkCore;
using TodoList.Core.Repositories;
using TodoList.Core.SeedWorks;
using TodoList.Infrastructure.Data;
using TodoList.Infrastructure.Repositories;
using TodoList.Infrastructure.SeedWorks;
using TodoList.UseCases;
using TodoList.UseCases.Implements;
using TodoList.Presentation.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TodoListDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<ITodoListManager, TodoListManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseGlobalExceptionMiddleware();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
