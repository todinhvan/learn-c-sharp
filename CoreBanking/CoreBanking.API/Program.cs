using CoreBanking.API.Apis;
using CoreBanking.API.Bootstrapping;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

// Global exception handler middleware
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map health check endpoints (/health, /alive)
app.MapDefaultEndpoints();

app.MapCoreBankingApi();

app.Run();
