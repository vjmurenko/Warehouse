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

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<WarehouseDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    // The IMediator will be injected separately through the constructor
});

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetBalancesQuery).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Register repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBalanceRepository, BalanceRepository>();
builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<INamedEntityRepository<Resource>, ResourceRepository>();
builder.Services.AddScoped<INamedEntityRepository<Client>, ClientRepository>();
builder.Services.AddScoped<INamedEntityRepository<UnitOfMeasure>, UnitOfMeasureRepository>();

// Register services
builder.Services.AddScoped<IBalanceService, BalanceService>();
builder.Services.AddScoped<INamedEntityValidationService, NamedEntityValidationService>();
builder.Services.AddScoped<IShipmentValidationService, ShipmentValidationService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();

var app = builder.Build();

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse Management API V1");
        c.RoutePrefix = "swagger"; 
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapControllers();

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    try
    {
        await context.Database.MigrateAsync();
        // Log successful migration
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

// Make Program class accessible for testing
namespace WarehouseManagement.Web
{
    public partial class Program { }
}
