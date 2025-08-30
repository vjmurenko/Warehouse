using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Common;
using WarehouseManagement.Application.Repositories;
using WarehouseManagement.Application.Services.Interfaces;
using WarehouseManagement.Application.Services.Implementations;
using WarehouseManagement.Domain.Aggregates.NamedAggregates;
using WarehouseManagement.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(WarehouseManagement.Application.Features.Balances.Queries.GetBalances.GetBalancesQuery).Assembly);
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
builder.Services.AddScoped<IReceiptDocumentService, ReceiptDocumentService>();
builder.Services.AddScoped<IReceiptValidationService, ReceiptValidationService>();
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
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

app.Run();

// Make Program class accessible for testing
public partial class Program { }
