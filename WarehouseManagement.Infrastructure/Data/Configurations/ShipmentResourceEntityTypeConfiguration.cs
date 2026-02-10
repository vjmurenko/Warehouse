using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Infrastructure.Data.Configurations;

public sealed class ShipmentResourceEntityTypeConfiguration : IEntityTypeConfiguration<ShipmentResource>
{
    public void Configure(EntityTypeBuilder<ShipmentResource> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.ShipmentDocumentId).IsRequired();
        builder.Property(e => e.ResourceId).IsRequired();
        builder.Property(e => e.UnitOfMeasureId).IsRequired();
        builder.Property(e => e.Quantity).HasColumnType("decimal(18,6)");

        builder.HasOne(e => e.UnitOfMeasure).WithMany().HasForeignKey(c => c.UnitOfMeasureId);
        builder.HasOne(e => e.Resource).WithMany().HasForeignKey(c => c.ResourceId);
    }
}