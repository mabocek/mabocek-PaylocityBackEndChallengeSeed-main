using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Api.Data;
using Xunit;

namespace ApiTests;

/// <summary>
/// Base class for integration tests using WebApplicationFactory.
/// This approach is robust for CI/CD environments as it doesn't require external processes.
/// All integration tests should inherit from this class.
/// </summary>
[Trait("Category", "Integration")]
public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient HttpClient;

    protected IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            // Override services for testing if needed
            builder.ConfigureServices(services =>
            {
                // Replace the DbContext with a unique database per test class
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Use a unique database name for each test class to avoid data pollution
                var databaseName = $"TestDb_{GetType().Name}_{Guid.NewGuid()}";
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
            });
        });

        HttpClient = Factory.CreateClient();
        HttpClient.DefaultRequestHeaders.Add("accept", "text/plain");
    }

    public void Dispose()
    {
        HttpClient?.Dispose();
        Factory?.Dispose();
    }
}
