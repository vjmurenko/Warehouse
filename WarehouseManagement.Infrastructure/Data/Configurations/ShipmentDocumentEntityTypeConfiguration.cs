using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

namespace WarehouseManagement.Infrastructure.Data.Configurations;

public sealed class ShipmentDocumentEntityTypeConfiguration : IEntityTypeConfiguration<ShipmentDocument>
{
    public void Configure(EntityTypeBuilder<ShipmentDocument> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Number).IsRequired().HasMaxLength(50);
        builder.Property(e => e.ClientId).IsRequired();
        builder.Property(e => e.Date).IsRequired();
        builder.Property(e => e.IsSigned).IsRequired();
        builder.HasIndex(e => e.Number).IsUnique();

        builder.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId);
        
        builder.HasMany(e => e.ShipmentResources)
            .WithOne()
            .HasForeignKey("ShipmentDocumentId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}