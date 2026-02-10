using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WarehouseManagement.Application.Features.Balances.Queries.GetBalances;
using WarehouseManagement.Infrastructure.Behaviors;
using WarehouseManagement.Infrastructure.Data;
using WarehouseManagement.Infrastructure.Extensions;
using WarehouseManagement.Infrastructure.Queries.Balances;
using WarehouseManagement.Web.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Настройка Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule<ReferenceHandlersModule>();
});

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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
    cfg.RegisterServicesFromAssembly(typeof(GetBalancesQueryHandler).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseExceptionHandler();

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
