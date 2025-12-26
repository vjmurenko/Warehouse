using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;

namespace WarehouseManagement.Infrastructure.Data.Configurations;

public sealed class ReceiptDocumentEntityTypeConfiguration : IEntityTypeConfiguration<ReceiptDocument>
{
    public void Configure(EntityTypeBuilder<ReceiptDocument> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Number).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Date).IsRequired();
        builder.HasIndex(e => e.Number).IsUnique();
        
        builder.HasMany(e => e.ReceiptResources)
            .WithOne()
            .HasForeignKey(r => r.ReceiptDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}