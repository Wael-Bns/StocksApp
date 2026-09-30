using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StocksApp.Domain.Entities;

namespace StocksApp.Infrastructure.Configurations
{
    public class Candle1mConfiguration : IEntityTypeConfiguration<Candle1m>
    {
        public void Configure(EntityTypeBuilder<Candle1m> builder)
        {
            builder.ToTable("candles_1m");

            builder.HasKey(c => new { c.Symbol, c.BucketStart });

            builder.Property(c => c.Symbol).HasColumnName("symbol").HasMaxLength(16).IsRequired();
            builder.Property(c => c.BucketStart).HasColumnName("bucket_start").IsRequired();
            builder.Property(c => c.Open).HasColumnName("open").HasColumnType("numeric(18,6)").IsRequired();
            builder.Property(c => c.High).HasColumnName("high").HasColumnType("numeric(18,6)").IsRequired();
            builder.Property(c => c.Low).HasColumnName("low").HasColumnType("numeric(18,6)").IsRequired();
            builder.Property(c => c.Close).HasColumnName("close").HasColumnType("numeric(18,6)").IsRequired();
            builder.Property(c => c.Volume).HasColumnName("volume").IsRequired();
            builder.Property(c => c.TradeCount).HasColumnName("trade_count").IsRequired();
        }
    }
}