using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Aggregates;

namespace WarehouseManagement.Infrastructure.Data.Configurations;

public sealed class StockMovementEntityTypeConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ResourceId).IsRequired();
        builder.Property(e => e.UnitOfMeasureId).IsRequired();
        builder.Property(e => e.Quantity).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(e => e.DocumentId).IsRequired();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.DocumentId);
        builder.HasIndex(e => new { e.ResourceId, e.UnitOfMeasureId });
    }
}
