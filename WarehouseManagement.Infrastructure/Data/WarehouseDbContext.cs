using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;

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

        // Конфигурация для Named Entities
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.IsActive).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<UnitOfMeasure>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.IsActive).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.IsActive).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
            entity.OwnsOne(e => e.Address);
        });

        // Конфигурация для Balance
        modelBuilder.Entity<Balance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ResourceId).IsRequired();
            entity.Property(e => e.UnitOfMeasureId).IsRequired();
            entity.OwnsOne(e => e.Quantity, q => 
            {
                q.Property(p => p.Value).HasColumnName("Quantity").HasColumnType("decimal(18,6)");
            });
            entity.HasIndex(e => new { e.ResourceId, e.UnitOfMeasureId }).IsUnique();
        });

        // Конфигурация для ReceiptDocument
        modelBuilder.Entity<ReceiptDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Date).IsRequired();
            entity.HasIndex(e => e.Number).IsUnique();
            entity.HasMany(e => e.ReceiptResources)
                .WithOne()
                .HasForeignKey(r => r.ReceiptDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Конфигурация для ReceiptResource
        modelBuilder.Entity<ReceiptResource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReceiptDocumentId).IsRequired();
            entity.Property(e => e.ResourceId).IsRequired();
            entity.Property(e => e.UnitOfMeasureId).IsRequired();
            entity.OwnsOne(e => e.Quantity, q => 
            {
                q.Property(p => p.Value).HasColumnName("Quantity").HasColumnType("decimal(18,6)");
            });
        });

        // Конфигурация для ShipmentDocument
        modelBuilder.Entity<ShipmentDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ClientId).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.IsSigned).IsRequired();
            entity.HasIndex(e => e.Number).IsUnique();
            entity.HasMany(e => e.ShipmentResources)
                .WithOne()
                .HasForeignKey("ShipmentDocumentId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Конфигурация для ShipmentResource
        modelBuilder.Entity<ShipmentResource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShipmentDocumentId).IsRequired();
            entity.Property(e => e.ResourceId).IsRequired();
            entity.Property(e => e.UnitOfMeasureId).IsRequired();
            entity.OwnsOne(e => e.Quantity, q => 
            {
                q.Property(p => p.Value).HasColumnName("Quantity").HasColumnType("decimal(18,6)");
            });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Database.EnsureCreatedAsync(cancellationToken);
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            throw;
        }
    }
}
