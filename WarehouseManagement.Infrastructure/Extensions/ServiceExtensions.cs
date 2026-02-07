using Microsoft.Extensions.DependencyInjection;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Domain.Aggregates.ReferenceAggregates;
using WarehouseManagement.Infrastructure.Repositories;
using WarehouseManagement.Infrastructure.Repositories.Common;
using WarehouseManagement.Infrastructure.Services;

namespace WarehouseManagement.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<IReferenceRepository<Resource>, ResourceRepository>();
        services.AddScoped<IReferenceRepository<Client>, ClientRepository>();
        services.AddScoped<IReferenceRepository<UnitOfMeasure>, UnitOfMeasureRepository>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IReferenceValidationService, ReferenceValidationService>();
        
        return services;
    }
}