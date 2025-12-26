using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Aggregates;

namespace WarehouseManagement.Infrastructure.Data.Configurations;

public sealed class BalanceEntityTypeConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ResourceId).IsRequired();
        builder.Property(e => e.UnitOfMeasureId).IsRequired();
        
        builder.OwnsOne(e => e.Quantity, q => 
        {
            q.Property(p => p.Value).HasColumnName("Quantity").HasColumnType("decimal(18,6)");
        });
        
        builder.HasIndex(e => new { e.ResourceId, e.UnitOfMeasureId }).IsUnique();
    }
}