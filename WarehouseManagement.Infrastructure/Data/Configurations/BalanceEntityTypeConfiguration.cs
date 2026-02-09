using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Aggregates.BalanceAggregate;

namespace WarehouseManagement.Infrastructure.Data.Configurations;

public sealed class BalanceEntityTypeConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ResourceId).IsRequired();
        builder.Property(e => e.UnitOfMeasureId).IsRequired();
        builder.Property(e => e.Quantity).HasColumnType("decimal(18,6)").IsRequired();

        builder.HasIndex(e => new { e.ResourceId, e.UnitOfMeasureId }).IsUnique();
    }
}
