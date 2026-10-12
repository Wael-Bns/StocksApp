using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StocksApp.Domain.Entities;

namespace StocksApp.Infrastructure.Configurations
{
    public class CandleMatchFlushBackupConfiguration : IEntityTypeConfiguration<CandleMatchFlushBackup>
    {
        public void Configure(EntityTypeBuilder<CandleMatchFlushBackup> builder)
        {
            builder.ToTable("candle_match_flush_backup");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Symbol).HasMaxLength(16).IsRequired();
            builder.Property(b => b.Open).HasColumnType("numeric(18,6)").IsRequired();
            builder.Property(b => b.High).HasColumnType("numeric(18,6)").IsRequired();
            builder.Property(b => b.Low).HasColumnType("numeric(18,6)").IsRequired();
            builder.Property(b => b.Close).HasColumnType("numeric(18,6)").IsRequired();
            builder.Property(b => b.Volume).IsRequired();
            builder.Property(b => b.TradeCount).IsRequired();
            builder.Property(b => b.FailedAt).IsRequired();
        }
    }
}
