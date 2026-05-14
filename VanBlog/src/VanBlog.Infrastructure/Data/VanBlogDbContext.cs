using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VanBlog.Core.Domain.Content;
using VanBlog.Core.Domain.Identity;

namespace VanBlog.Infrastructure.Data
{
    public class VanBlogDbContext : IdentityDbContext<User, Role, Guid>
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostActivityLog> PostActivityLogs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<PostSeries> PostSeries { get; set; }

        public VanBlogDbContext(DbContextOptions<VanBlogDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Post>(b =>
            {
                b.ToTable("Posts");
                b.HasIndex(p => p.Slug).IsUnique();
                b.HasKey(p => p.Id);
                b.Property(p => p.Name).IsRequired().HasMaxLength(250);
                b.Property(p => p.Slug).IsRequired().HasColumnType("varchar(250)");
                b.Property(p => p.Description).HasMaxLength(500);
                b.Property(p => p.CategoryId).IsRequired();
                b.Property(p => p.Thumbnail).HasMaxLength(500);
                b.Property(p => p.AuthorId).IsRequired();
                b.Property(p => p.Source).HasMaxLength(128);
                b.Property(p => p.Tags).HasMaxLength(250);
                b.Property(p => p.SeoDescription).HasMaxLength(160);
                b.Property(p => p.Status).HasDefaultValue(PostStatus.Draft);
            });

            builder.Entity<PostActivityLog>(b =>
            {
                b.ToTable("PostActivityLogs");
                b.HasKey(p => p.Id);
                b.Property(p => p.PostId).IsRequired();
                b.Property(p => p.Note).HasMaxLength(500);
                b.Property(p => p.UserId).IsRequired();
            });

            builder.Entity<Category>(b =>
            {
                b.ToTable("Categories");
                b.HasIndex(c => c.Slug).IsUnique();
                b.HasKey(c => c.Id);
                b.Property(c => c.Name).IsRequired().HasMaxLength(250);
                b.Property(c => c.Slug).IsRequired().HasColumnType("varchar(250)");
                b.Property(c => c.SeoDescription).HasMaxLength(160);
            });

            builder.Entity<Tag>(b =>
            {
                b.ToTable("Tags");
                b.HasKey(t => t.Id);
                b.Property(t => t.Name).IsRequired().HasMaxLength(100);
            });

            builder.Entity<PostTag>(b =>
            {
                b.ToTable("PostTags");
                b.HasKey(p => new { p.PostId, p.TagId }); // new {id, id}: Set composite primary key
            });

            builder.Entity<Series>(b =>
            {
                b.ToTable("Series");
                b.HasIndex(s => s.Slug).IsUnique();
                b.HasKey(s => s.Id);
                b.Property(s => s.Name).IsRequired().HasMaxLength(250);
                b.Property(s => s.Slug).IsRequired().HasColumnType("varchar(250)");
                b.Property(s => s.Description).HasMaxLength(500);
                b.Property(s => s.SeoDescription).HasMaxLength(160);
                b.Property(s => s.Thumbnail).HasMaxLength(500);
                b.Property(s => s.AuthorId).IsRequired();
            });

            builder.Entity<PostSeries>(b =>
            {
                b.ToTable("PostSeries");
                b.HasKey(p => new { p.PostId, p.SeriesId });
            });

            builder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(u => u.Id);
                b.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                b.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                b.Property(u => u.Avatar).HasMaxLength(500);
            });

            builder.Entity<Role>(b =>
            {
                b.ToTable("Roles");
                b.HasKey(r => r.Id);
                b.Property(r => r.Name).IsRequired().HasMaxLength(250);
            });

            builder.Entity<IdentityUserRole<Guid>>(b =>
            {
                b.ToTable("UserRoles");
                b.HasKey(ur => new { ur.UserId, ur.RoleId });
            });

            builder.Entity<IdentityUserClaim<Guid>>(b =>
            {
                b.ToTable("UserClaims");
                b.HasKey(uc => uc.Id);
            });

            builder.Entity<IdentityRoleClaim<Guid>>(b =>
            {
                b.ToTable("RoleClaims");
                b.HasKey(rc => rc.Id);
            });

            builder.Entity<IdentityUserLogin<Guid>>(b =>
            {
                b.ToTable("UserLogins");
                b.HasKey(ul => ul.UserId);
            });

            builder.Entity<IdentityUserToken<Guid>>(b =>
            {
                b.ToTable("UserTokens");
                b.HasKey(ut => ut.UserId);
            });

            builder.Ignore<IdentityPasskeyData>();
            builder.Ignore<IdentityUserPasskey<Guid>>();
        }

        //public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        //{
        //    var entries = ChangeTracker
        //        .Entries()
        //        .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
        //    var now = DateTime.Now;

        //    foreach (var entityEntry in entries)
        //    {
        //        var createdAtProp = entityEntry.Entity.GetType().GetProperty("CreatedAt");
        //        if (entityEntry.State == EntityState.Added
        //            && createdAtProp != null)
        //        {
        //            createdAtProp.SetValue(entityEntry.Entity, now);
        //        }
        //        var updatedAtProp = entityEntry.Entity.GetType().GetProperty("UpdatedAt");
        //        if (entityEntry.State == EntityState.Modified
        //            && updatedAtProp != null)
        //        {
        //            updatedAtProp.SetValue(entityEntry.Entity, now);
        //        }
        //    }
        //    return base.SaveChangesAsync(cancellationToken);
        //}
    }
}
