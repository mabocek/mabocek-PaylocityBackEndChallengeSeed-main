using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Api.Models;
using Api.Dtos.Employee;
using Api.Dtos.Dependent;

namespace ApiTests.IntegrationTests;

/// <summary>
/// Integration tests for pagination functionality
/// Tests the newly implemented pagination endpoints
/// </summary>
public class PaginationIntegrationTests : IntegrationTestBase
{
    public PaginationIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Feature", "Pagination")]
    public async Task GetPagedEmployees_WithValidParameters_ShouldReturnPagedResult()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/api/v1/employees?page=1&pageSize=2");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<GetEmployeeDto>>(content, _jsonOptions);

        Assert.NotNull(pagedResult);
        Assert.Equal(1, pagedResult.CurrentPage);
        Assert.Equal(2, pagedResult.PageSize);
        Assert.True(pagedResult.Items.Count <= 2); // Should return at most 2 items
        Assert.True(pagedResult.TotalItems >= 0); // Should have some total count
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Feature", "Pagination")]
    public async Task GetPagedEmployees_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange & Act - sort by first name ascending
        var response = await HttpClient.GetAsync("/api/v1/employees?page=1&pageSize=10&sortBy=firstname&sortOrder=asc");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<GetEmployeeDto>>(content, _jsonOptions);

        Assert.NotNull(pagedResult);

        // Verify sorting if we have multiple items
        if (pagedResult.Items.Count > 1)
        {
            for (int i = 0; i < pagedResult.Items.Count - 1; i++)
            {
                var current = pagedResult.Items[i].FirstName;
                var next = pagedResult.Items[i + 1].FirstName;
                Assert.True(string.Compare(current, next, StringComparison.OrdinalIgnoreCase) <= 0);
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Feature", "Pagination")]
    public async Task GetPagedDependents_WithValidParameters_ShouldReturnPagedResult()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/api/v1/dependents?page=1&pageSize=3");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<GetDependentDto>>(content, _jsonOptions);

        Assert.NotNull(pagedResult);
        Assert.Equal(1, pagedResult.CurrentPage);
        Assert.Equal(3, pagedResult.PageSize);
        Assert.True(pagedResult.Items.Count <= 3); // Should return at most 3 items
        Assert.True(pagedResult.TotalItems >= 0); // Should have some total count
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Feature", "Pagination")]
    public async Task GetPagedDependents_WithFiltering_ShouldReturnFilteredResults()
    {
        // Arrange & Act - filter by relationship (assuming we have some test data)
        var response = await HttpClient.GetAsync("/api/v1/dependents?page=1&pageSize=10&relationship=Child");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<GetDependentDto>>(content, _jsonOptions);

        Assert.NotNull(pagedResult);

        // Verify filtering - all returned dependents should be children (if any)
        foreach (var dependent in pagedResult.Items)
        {
            Assert.Equal(Relationship.Child, dependent.Relationship);
        }
    }

    [Theory]
    [Trait("Category", "Integration")]
    [Trait("Feature", "Pagination")]
    [InlineData(0, 10)] // Invalid page number
    [InlineData(1, 0)]  // Invalid page size
    [InlineData(1, 101)] // Page size too large
    public async Task GetPagedEmployees_WithInvalidParameters_ShouldHandleGracefully(int page, int pageSize)
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync($"/api/v1/employees?page={page}&pageSize={pageSize}");

        // Assert
        // Should either return validation error or handle gracefully
        // The implementation should validate parameters and return appropriate response
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Feature", "Pagination")]
    public async Task GetPagedEmployees_DefaultParameters_ShouldUseDefaults()
    {
        // Arrange & Act - no parameters, should use defaults
        var response = await HttpClient.GetAsync("/api/v1/employees");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<GetEmployeeDto>>(content, _jsonOptions);

        Assert.NotNull(pagedResult);
        Assert.Equal(1, pagedResult.CurrentPage); // Default page should be 1
        Assert.Equal(10, pagedResult.PageSize); // Default page size should be 10
    }
}
