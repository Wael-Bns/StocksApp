using Microsoft.EntityFrameworkCore;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<BuyOrder> BuyOrders { get; set; }
        public DbSet<SellOrder> SellOrders { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // BuyOrder table configuration
            modelBuilder.Entity<BuyOrder>()
                .ToTable("BuyOrder")
                .HasOne(b => b.User)
                .WithMany(u => u.BuyOrders)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // SellOrder table configuration
            modelBuilder.Entity<SellOrder>()
                .ToTable("SellOrder")
                .HasOne(s => s.User)
                .WithMany(u => u.SellOrders)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // ApplicationUser table configuration
            modelBuilder.Entity<ApplicationUser>()
                .ToTable("ApplicationUser");
        }
    }
}
