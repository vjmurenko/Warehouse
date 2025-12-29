using MediatR;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Aggregates;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReceiptAggregate;
using WarehouseManagement.Domain.Aggregates.ShipmentAggregate;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Data.Configurations;
using WarehouseManagement.Infrastructure.Extensions;

namespace WarehouseManagement.Infrastructure.Data;

public sealed class WarehouseDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;

    public WarehouseDbContext()
    {
        
    }
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Resource> Resources { get; set; }
    public DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ReceiptDocument> ReceiptDocuments { get; set; }
    public DbSet<ReceiptResource> ReceiptResources { get; set; }
    public DbSet<ShipmentDocument> ShipmentDocuments { get; set; }
    public DbSet<ShipmentResource> ShipmentResources { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Ignore domain event types - they are not persisted
        modelBuilder.Ignore<Event>();
        
        modelBuilder.ApplyConfiguration(new ResourceEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new UnitOfMeasureEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ClientEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ReceiptDocumentEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ReceiptResourceEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ShipmentDocumentEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ShipmentResourceEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new StockMovementEntityTypeConfiguration());
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch Domain Events collection. 
        // Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
        // side effects from the domain event handlers which are using the same DbContext with "scoped" lifetime
        if (_mediator is not null)
        {
            await _mediator.DispatchDomainEventsAsync(this);
        }

        // After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
        // performed through the DbContext will be committed
        _ = await base.SaveChangesAsync(cancellationToken);

        return true;
    }

    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
