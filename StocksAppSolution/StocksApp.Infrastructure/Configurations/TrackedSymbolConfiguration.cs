// StocksApp.Infrastructure/Configurations/TrackedSymbolConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StocksApp.Domain.Entities;

namespace StocksApp.Infrastructure.Configurations
{
    public class TrackedSymbolConfiguration : IEntityTypeConfiguration<TrackedSymbol>
    {
        public void Configure(EntityTypeBuilder<TrackedSymbol> builder)
        {
            builder.ToTable("TrackedSymbols");

            builder.HasKey(t => t.Symbol);

            builder.Property(t => t.Symbol)
                .HasMaxLength(25)          // same as BuyOrder.StockSymbol
                .IsRequired();

            builder.Property(t => t.DisplayName)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(t => t.IsActive)
                .HasDefaultValue(true)
                .HasSentinel(true)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(t => t.UpdatedAt)
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_TrackedSymbols_Symbol_Upper", "\"Symbol\" = upper(\"Symbol\")"));
        }
    }
}