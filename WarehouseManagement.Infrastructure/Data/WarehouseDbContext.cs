using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Infrastructure.Data.Configurations;

namespace WarehouseManagement.Infrastructure.Data;

public class WarehouseDbContext : DbContext
{

    public WarehouseDbContext()
    {
        
    }
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options)
    {
    }

    public DbSet<Resource> Resources { get; set; }
    public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Balance> Balances { get; set; }
    public DbSet<ReceiptDocument> ReceiptDocuments { get; set; }
    public DbSet<ReceiptResource> ReceiptResources { get; set; }
    public DbSet<ShipmentDocument> ShipmentDocuments { get; set; }
    public DbSet<ShipmentResource> ShipmentResources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new ResourceEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UnitOfMeasureEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ClientEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new BalanceEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ReceiptDocumentEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ReceiptResourceEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ShipmentDocumentEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ShipmentResourceEntityTypeConfiguration());
    }
}
