using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Infrastructure.Data.Configurations;

public sealed class ReceiptResourceEntityTypeConfiguration : IEntityTypeConfiguration<ReceiptResource>
{
    public void Configure(EntityTypeBuilder<ReceiptResource> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.ReceiptDocumentId).IsRequired();
        builder.Property(e => e.ResourceId).IsRequired();
        builder.Property(e => e.UnitOfMeasureId).IsRequired();
        builder.Property(e => e.Quantity).HasColumnType("decimal(18,6)");

        builder.HasOne(e => e.Resource).WithMany().HasForeignKey(e => e.ResourceId);
        builder.HasOne(e => e.UnitOfMeasure).WithMany().HasForeignKey(e => e.UnitOfMeasureId);
    }
}