using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using VanBlog.Api;
using VanBlog.Api.Middlewares;
using VanBlog.Api.Services;
using VanBlog.Api.Services.Implement;
using VanBlog.Core.Domain.Identity;
using VanBlog.Core.Mappings;
using VanBlog.Core.Repositories;
using VanBlog.Core.SeedWorks;
using VanBlog.Infrastructure.Data;
using VanBlog.Infrastructure.Repositories;
using VanBlog.Infrastructure.SeedWorks;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
//Config DB Context and ASP.NET Core Identity
builder.Services.AddDbContext<VanBlogDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddIdentity<User, Role>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<VanBlogDbContext>();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 4;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

// Add Services
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IPostsService, PostsService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();

// Add Mappers
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<MappingProfile>();
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomOperationIds(apiDescription =>
    {
        return apiDescription.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null;
    });
    options.SwaggerDoc("AdminAPI", new Microsoft.OpenApi.OpenApiInfo
    {
        Version = "v1",
        Title = "Admin API for Van Blog",
        Description = "API for CMS core domain. This domain keeps track of campaigns, campaign rules, and campaign execution.",
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("AdminAPI/swagger.json", "Admin API");
        options.DisplayOperationId();
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();

app.MapControllers();

await app.MigrateDatabase();

app.Run();
