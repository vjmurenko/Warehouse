using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.IntegrationTests.Infrastructure;

public class TestcontainersWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("warehouse_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the default DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<WarehouseDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test database context with testcontainers connection string
            services.AddDbContext<WarehouseDbContext>(options =>
            {
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
            });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}