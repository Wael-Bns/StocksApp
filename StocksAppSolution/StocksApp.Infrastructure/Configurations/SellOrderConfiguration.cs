using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StocksApp.Core.Domain.Entities;
using StocksApp.Core.Enums;

namespace StocksApp.Infrastructure.Configurations
{
    public class SellOrderConfiguration : IEntityTypeConfiguration<SellOrder>
    {
        public void Configure(EntityTypeBuilder<SellOrder> builder)
        {
            builder.ToTable("SellOrder");
            builder.HasKey(b => b.SellOrderID);

            builder.Property(b => b.StockSymbol)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(b => b.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.Quantity)
                .IsRequired();

            builder.ToTable(t => t.HasCheckConstraint("CK_SellOrder_Quantity", "\"Quantity\" > 0"));
            builder.ToTable(t => t.HasCheckConstraint("CK_SellOrder_Price", "\"Price\" > 0"));
        }
    }
}
