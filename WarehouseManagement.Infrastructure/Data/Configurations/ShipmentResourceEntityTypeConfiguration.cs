using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Infrastructure.Data.Configurations;

public class ShipmentResourceEntityTypeConfiguration : IEntityTypeConfiguration<ShipmentResource>
{
    public void Configure(EntityTypeBuilder<ShipmentResource> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ShipmentDocumentId).IsRequired();
        builder.Property(e => e.ResourceId).IsRequired();
        builder.Property(e => e.UnitOfMeasureId).IsRequired();
        
        builder.OwnsOne(e => e.Quantity, q => 
        {
            q.Property(p => p.Value).HasColumnName("Quantity").HasColumnType("decimal(18,6)");
        });
    }
}