using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StocksApp.Domain.Entities;

namespace StocksApp.Infrastructure.Configurations
{
    public class OutboxConfiguration : IEntityTypeConfiguration<Outbox>
    {
        public void Configure(EntityTypeBuilder<Outbox> builder)
        {
            builder.ToTable("Outbox");

            builder.HasKey(o => o.OutboxId);

            builder.Property(o => o.EventType)
                .HasConversion(
                    t => t.AssemblyQualifiedName!,
                    s => Type.GetType(s)!)
                .IsRequired();

            builder.Property(o => o.Payload)
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(o => o.CreatedAt)
                .HasDefaultValueSql("now()")
                .IsRequired();
        }
    }
}
