using System;
using System.Net.Http;
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

    /// <summary>
    /// Gets an HTTP client configured for testing the API.
    /// Base address is set to https://localhost:7124
    /// </summary>
    protected HttpClient HttpClient
    {
        get
        {
            if (_httpClient == default)
            {
                _httpClient = new HttpClient
                {
                    //task: update your port if necessary
                    BaseAddress = new Uri("https://localhost:7124")
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

