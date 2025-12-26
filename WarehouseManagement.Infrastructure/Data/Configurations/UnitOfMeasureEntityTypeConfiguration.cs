using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Infrastructure.Data.Configurations;

public sealed class UnitOfMeasureEntityTypeConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.IsActive).IsRequired();
        builder.HasIndex(e => e.Name).IsUnique();
    }
}