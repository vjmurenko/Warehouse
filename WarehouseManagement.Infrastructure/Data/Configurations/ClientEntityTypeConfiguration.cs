using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;

namespace WarehouseManagement.Infrastructure.Data.Configurations;

public class ClientEntityTypeConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.IsActive).IsRequired();
        builder.HasIndex(e => e.Name).IsUnique();
        
        builder.OwnsOne(e => e.Address, a => 
        {
            a.Property(p => p.Name).HasColumnName("Address").HasMaxLength(500);
        });
    }
}