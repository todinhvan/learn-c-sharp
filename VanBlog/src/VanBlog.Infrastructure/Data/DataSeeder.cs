using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VanBlog.Core.Domain.Identity;

namespace VanBlog.Infrastructure.Data
{
    public class DataSeeder
    {
        public static async Task SeedAsync(VanBlogDbContext context, ILogger<DataSeeder> logger)
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADMIN");
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    DisplayName = "Quản trị viên"
                };
                await context.Roles.AddAsync(adminRole);
                await context.SaveChangesAsync();
                logger.LogInformation("Admin role created.");
            }

            if ((await context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == "ADMIN@GMAIL.COM")) == null)
            {
                var passwordHasher = new PasswordHasher<User>();
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Van",
                    LastName = "To",
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@gmail.com",
                    NormalizedEmail = "ADMIN@GMAIL.COM",
                    IsActive = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    LockoutEnabled = false,
                    CreatedAt = DateTime.Now
                };
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");
                await context.Users.AddAsync(adminUser);
                await context.UserRoles.AddAsync(new IdentityUserRole<Guid>
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Admin user created.");
            }
        }
    }
}
