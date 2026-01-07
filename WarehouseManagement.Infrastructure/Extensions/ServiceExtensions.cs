using Microsoft.Extensions.DependencyInjection;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Repositories;
using WarehouseManagement.Infrastructure.Repositories.Common;

namespace WarehouseManagement.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<INamedEntityRepository<Resource>, ResourceRepository>();
        services.AddScoped<INamedEntityRepository<Client>, ClientRepository>();
        services.AddScoped<INamedEntityRepository<UnitOfMeasure>, UnitOfMeasureRepository>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<INamedEntityValidationService, NamedEntityValidationService>();
        services.AddScoped<IShipmentValidationService, ShipmentValidationService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();
        
        return services;
    }
}