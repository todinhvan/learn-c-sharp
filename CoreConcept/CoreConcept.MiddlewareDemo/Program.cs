using CoreConcept.MiddlewareDemo.Middlewares;
using CoreConcept.MiddlewareDemo.Models;
using CoreConcept.MiddlewareDemo.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Configuration.AddJsonFile("my-appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"my-appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddScoped<IClientInfoRepository, ClientInfoRepository>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseApiKeyMiddleware();

app.UseAuthorization();

app.MapControllers();

app.Run();
