using System.Data.Common;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebAPI.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    
    private readonly DbConnection _connection = new SqliteConnection("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Environment", "Testing");

        builder.ConfigureServices(services =>
        {
           
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(AppDbContext) ||
                            d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseSqlite(_connection);
            });

           
            services.AddScoped<IClientRepository, ClientRepository>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        return base.CreateHost(builder);
    }

    public CustomWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Close();
            _connection.Dispose();
        }
        base.Dispose(disposing);
    }
}