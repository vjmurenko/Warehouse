using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Infrastructure.Data.Configurations;

public class ReceiptResourceEntityTypeConfiguration : IEntityTypeConfiguration<ReceiptResource>
{
    public void Configure(EntityTypeBuilder<ReceiptResource> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ReceiptDocumentId).IsRequired();
        builder.Property(e => e.ResourceId).IsRequired();
        builder.Property(e => e.UnitOfMeasureId).IsRequired();
        
        builder.OwnsOne(e => e.Quantity, q => 
        {
            q.Property(p => p.Value).HasColumnName("Quantity").HasColumnType("decimal(18,6)");
        });
    }
}