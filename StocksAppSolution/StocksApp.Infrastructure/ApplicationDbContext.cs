using Microsoft.EntityFrameworkCore;
using StocksApp.Domain.Entities;

namespace StocksApp.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<BuyOrder> BuyOrders { get; set; }
        public DbSet<SellOrder> SellOrders { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Outbox> Outbox { get; set; }
        public DbSet<TrackedSymbol> TrackedSymbols { get; set; }
        public DbSet<Candle1m> Candles1m { get; set; }
        public DbSet<CandleMatch5s> CandleMatches5s { get; set; }
        public DbSet<CandleMatchFlushBackup> CandleMatchFlushBackups { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
