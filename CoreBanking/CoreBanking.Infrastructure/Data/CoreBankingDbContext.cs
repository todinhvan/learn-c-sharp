using CoreBanking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.Infrastructure.Data
{
    public class CoreBankingDbContext(DbContextOptions<CoreBankingDbContext> options) : DbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.HasMany(e => e.Accounts)
                      .WithOne(a => a.Customer)
                      .HasForeignKey(a => a.CustomerId);
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Accounts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
                entity.HasOne(a => a.Customer)
                      .WithMany(c => c.Accounts)
                      .HasForeignKey(a => a.CustomerId);
                entity.HasMany(a => a.Transactions)
                      .WithOne(t => t.Account)
                      .HasForeignKey(t => t.AccountId);

                // Optimistic concurrency — PostgreSQL uses xmin system column
                entity.Property(e => e.RowVersion)
                      .IsRowVersion();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.ReferenceId);
                entity.HasOne(t => t.Account)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(t => t.AccountId);
            });
        }
    }
}
