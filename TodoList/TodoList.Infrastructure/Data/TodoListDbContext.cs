using Microsoft.EntityFrameworkCore;
using TodoList.Core.Entities;

namespace TodoList.Infrastructure.Data
{
    public class TodoListDbContext(DbContextOptions<TodoListDbContext> options) : DbContext(options)
    {
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Item>(entity =>
            {
                entity.ToTable("Items");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.IsCompleted).HasDefaultValue(false);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entities = ChangeTracker.Entries<AuditableEntity>();
            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    entity.Entity.CreatedAt = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
