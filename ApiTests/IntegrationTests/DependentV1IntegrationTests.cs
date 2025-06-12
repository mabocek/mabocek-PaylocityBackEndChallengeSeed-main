using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiTests.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Feature", "Dependents")]
public class DependentV1IntegrationTests : IntegrationTestBase
{
    public DependentV1IntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }
    [Fact]
    [Trait("TestType", "GetAll")]
    [Trait("Priority", "High")]
    //task: make test pass
    public async Task WhenAskedForAllDependents_ShouldReturnAllDependents()
    {
        var response = await HttpClient.GetAsync("/api/v1/dependents");
        var dependents = new List<GetDependentDto>
        {
            new(1, "Spouse", "Morant", new DateOnly(1998, 3, 3), Relationship.Spouse, 2),
            new(2, "Child1", "Morant", new DateOnly(2020, 6, 23), Relationship.Child, 2),
            new(3, "Child2", "Morant", new DateOnly(2021, 5, 18), Relationship.Child, 2),
            new(4, "DP", "Jordan", new DateOnly(1974, 1, 2), Relationship.DomesticPartner, 3)
        };

        // Construct expected paginated result
        var expectedPagedResult = new PagedResult<GetDependentDto>(dependents, 4, 1, 10);

        await response.ShouldReturn(HttpStatusCode.OK, expectedPagedResult);
    }

    [Fact]
    [Trait("TestType", "GetById")]
    [Trait("Priority", "High")]
    //task: make test pass
    public async Task WhenAskedForADependent_ShouldReturnCorrectDependent()
    {
        var response = await HttpClient.GetAsync("/api/v1/dependents/1");
        var dependent = new GetDependentDto(1, "Spouse", "Morant", new DateOnly(1998, 3, 3), Relationship.Spouse, 2);
        await response.ShouldReturn(HttpStatusCode.OK, dependent);
    }

    /// <summary>
    /// Tests error handling for various non-existent dependent scenarios
    /// </summary>
    [Theory]
    [Trait("TestType", "ErrorHandling")]
    [Trait("Priority", "Medium")]
    [Trait("ExpectedResult", "NotFound")]
    [InlineData(int.MinValue)] // Minimum integer value
    [InlineData(-1)] // Negative ID
    [InlineData(0)] // Zero ID
    [InlineData(999999)] // Large non-existent ID
    [InlineData(100)] // Reasonable but non-existent ID
    public async Task WhenAskedForANonexistentDependent_ShouldReturn404(int nonExistentDependentId)
    {
        var response = await HttpClient.GetAsync($"/api/v1/dependents/{nonExistentDependentId}");
        await response.ShouldReturn(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("TestType", "Filtering")]
    [Trait("Priority", "High")]
    //task: test filtering by employeeId
    public async Task WhenAskedForDependentsByEmployeeId_ShouldReturnFilteredResults()
    {
        var response = await HttpClient.GetAsync("/api/v1/dependents?employeeId=2");
        var expectedDependents = new List<GetDependentDto>
        {
            new(1, "Spouse", "Morant", new DateOnly(1998, 3, 3), Relationship.Spouse, 2),
            new(2, "Child1", "Morant", new DateOnly(2020, 6, 23), Relationship.Child, 2),
            new(3, "Child2", "Morant", new DateOnly(2021, 5, 18), Relationship.Child, 2)
        };

        // Construct expected paginated result
        var expectedPagedResult = new PagedResult<GetDependentDto>(expectedDependents, 3, 1, 10);

        await response.ShouldReturn(HttpStatusCode.OK, expectedPagedResult);
    }

    [Fact]
    [Trait("TestType", "Filtering")]
    [Trait("Priority", "High")]
    //task: test filtering by relationship
    public async Task WhenAskedForDependentsByRelationship_ShouldReturnFilteredResults()
    {
        var response = await HttpClient.GetAsync("/api/v1/dependents?relationship=Child");
        var expectedDependents = new List<GetDependentDto>
        {
            new(2, "Child1", "Morant", new DateOnly(2020, 6, 23), Relationship.Child, 2),
            new(3, "Child2", "Morant", new DateOnly(2021, 5, 18), Relationship.Child, 2)
        };

        // Construct expected paginated result
        var expectedPagedResult = new PagedResult<GetDependentDto>(expectedDependents, 2, 1, 10);

        await response.ShouldReturn(HttpStatusCode.OK, expectedPagedResult);
    }

    [Fact]
    [Trait("TestType", "Sorting")]
    [Trait("Priority", "Medium")]
    //task: test sorting by firstName
    public async Task WhenAskedForDependentsSortedByFirstName_ShouldReturnSortedResults()
    {
        var response = await HttpClient.GetAsync("/api/v1/dependents?sortBy=firstname&ascending=true");
        var expectedDependents = new List<GetDependentDto>
        {
            new(2, "Child1", "Morant", new DateOnly(2020, 6, 23), Relationship.Child, 2),
            new(3, "Child2", "Morant", new DateOnly(2021, 5, 18), Relationship.Child, 2),
            new(4, "DP", "Jordan", new DateOnly(1974, 1, 2), Relationship.DomesticPartner, 3),
            new(1, "Spouse", "Morant", new DateOnly(1998, 3, 3), Relationship.Spouse, 2)
        };

        // Construct expected paginated result
        var expectedPagedResult = new PagedResult<GetDependentDto>(expectedDependents, 4, 1, 10);

        await response.ShouldReturn(HttpStatusCode.OK, expectedPagedResult);
    }

    [Fact]
    [Trait("TestType", "Combined")]
    [Trait("Priority", "High")]
    //task: test filtering and sorting combined
    public async Task WhenAskedForDependentsWithFiltersAndSorting_ShouldReturnFilteredAndSortedResults()
    {
        var response = await HttpClient.GetAsync("/api/v1/dependents?employeeId=2&sortBy=dateofbirth&sortOrder=desc");
        var expectedDependents = new List<GetDependentDto>
        {
            new(3, "Child2", "Morant", new DateOnly(2021, 5, 18), Relationship.Child, 2),
            new(2, "Child1", "Morant", new DateOnly(2020, 6, 23), Relationship.Child, 2),
            new(1, "Spouse", "Morant", new DateOnly(1998, 3, 3), Relationship.Spouse, 2)
        };

        // Construct expected paginated result
        var expectedPagedResult = new PagedResult<GetDependentDto>(expectedDependents, 3, 1, 10);

        await response.ShouldReturn(HttpStatusCode.OK, expectedPagedResult);
    }
}

