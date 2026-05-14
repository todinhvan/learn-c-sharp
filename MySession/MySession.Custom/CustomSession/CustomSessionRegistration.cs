namespace MySession.Custom.CustomSession
{
    public static class CustomSessionRegistration
    {
        public static IServiceCollection RegisterCustomSession(this IServiceCollection services)
        {
            services.AddScoped<ICustomSessionStorageEngine>(services =>
            {
                var path = Path.Combine(services.GetRequiredService<IHostEnvironment>().ContentRootPath, "sessions");
                Directory.CreateDirectory(path);
                return new InFileCustomSessionStorageEngine(path);
            });
            services.AddScoped<ICustomSessionStorage, CustomSessionStorage>();
            services.AddScoped<CustomSessionStorageCache>();
            return services;
        }
    }
}
