using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

IFormatProvider enUS = new CultureInfo("en-US");
IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "https://jwt.van.com",
                ValidAudience = "https://jwt.dinhvan.io",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("DinhVan2345@Van"))
            };

            options.Events = new JwtBearerEvents()
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Policy1", config =>
    {
        config.RequireRole("Role1");
        config.RequireClaim("client-id", "client1", "client2", "client3");
        //config.RequireUserName("To Dinh Van");
        config.RequireAssertion(context => context.User.Identity?.Name?.StartsWith("To", StringComparison.OrdinalIgnoreCase) ?? false);
        config.RequireClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth");
        config.RequireAssertion(context =>
            DateTime.TryParseExact(
                context.User.Claims
                    .Where(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth")
                    .Select(c => c.Value).FirstOrDefault(),
                "yyyy-MM-dd",
                enUS,
                DateTimeStyles.None,
                out DateTime dob
            ) && dob.Year < 2005
        );
    })
    .AddPolicy("Policy2", config => config.RequireRole("Role2").RequireRole("Role3"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
