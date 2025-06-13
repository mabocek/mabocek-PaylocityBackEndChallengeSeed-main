using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ApiTests;

/// <summary>
/// Base class for integration tests that provides HTTP client configuration
/// and common setup for testing API endpoints.
/// </summary>
[Trait("Category", "Integration")]
public abstract class IntegrationTest : IDisposable
{
    private HttpClient? _httpClient;
    private readonly IConfiguration _configuration;

    protected IntegrationTest()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: false)
            .Build();
    }

    /// <summary>
    /// Gets an HTTP client configured for testing the API.
    /// Base address is loaded from configuration
    /// </summary>
    protected HttpClient HttpClient
    {
        get
        {
            if (_httpClient == default)
            {
                var baseAddress = _configuration["TestConfiguration:BaseAddress"]
                    ?? throw new InvalidOperationException("TestConfiguration:BaseAddress not found in configuration");

                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri(baseAddress)
                };
                _httpClient.DefaultRequestHeaders.Add("accept", "text/plain");
            }

            return _httpClient;
        }
    }

    public void Dispose()
    {
        HttpClient.Dispose();
    }
}

