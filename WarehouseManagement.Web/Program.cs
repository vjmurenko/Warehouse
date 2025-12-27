using Microsoft.EntityFrameworkCore;
using Serilog;
using WarehouseManagement.Application.Behaviors;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Balances.Queries.GetBalances;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Repositories;
using WarehouseManagement.Infrastructure.Repositories.Common;
using WarehouseManagement.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<WarehouseDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetBalancesQuery).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
builder.Services.AddScoped<INamedEntityRepository<Resource>, ResourceRepository>();
builder.Services.AddScoped<INamedEntityRepository<Client>, ClientRepository>();
builder.Services.AddScoped<INamedEntityRepository<UnitOfMeasure>, UnitOfMeasureRepository>();

builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<INamedEntityValidationService, NamedEntityValidationService>();
builder.Services.AddScoped<IShipmentValidationService, ShipmentValidationService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Container"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse Management API V1");
        c.RoutePrefix = "swagger"; 
    });
}

app.UseCors("AllowFrontend");

app.MapHealthChecks("/health");

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    try
    {
        await context.Database.MigrateAsync();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WarehouseManagement.Web.Program>>();
        logger.LogInformation("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WarehouseManagement.Web.Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

app.Run();

namespace WarehouseManagement.Web
{
    public partial class Program { }
}
