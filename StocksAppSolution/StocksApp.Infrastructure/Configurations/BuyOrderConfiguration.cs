using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Infrastructure.Configurations
{
    public class BuyOrderConfiguration : IEntityTypeConfiguration<BuyOrder>
    {
        public void Configure(EntityTypeBuilder<BuyOrder> builder)
        {
            builder.ToTable("BuyOrder");
            
            builder.HasKey(b => b.BuyOrderID);
            
            builder.Property(b => b.StockSymbol)
                .HasMaxLength(10)
                .IsRequired();
            
            builder.Property(b => b.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.Quantity)
                .IsRequired();

            builder.ToTable(t => t.HasCheckConstraint("CK_BuyOrder_Quantity", "\"Quantity\" > 0"));
            builder.ToTable(t => t.HasCheckConstraint("CK_BuyOrder_Price", "\"Price\" > 0"));
        }
    }
}
