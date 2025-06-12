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

/// <summary>
/// Example of how to use WebApplicationFactory for more robust integration tests
/// This approach is recommended for CI/CD environments
/// </summary>
[Trait("Category", "Integration")]
[Trait("Feature", "Employees")]
public class EmployeeV1IntegrationTestsWebFactory : IntegrationTestBase
{
    public EmployeeV1IntegrationTestsWebFactory(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    [Trait("TestType", "GetAll")]
    [Trait("Priority", "High")]
    [Trait("DataComplexity", "Complex")]
    public async Task WhenAskedForAllEmployees_ShouldReturnAllEmployees_UsingWebFactory()
    {
        var response = await HttpClient.GetAsync("/api/v1/employees");

        var expectedEmployees = new List<GetEmployeeDto>
        {
            new(1, "LeBron", "James", 75420.99m, new DateOnly(1984, 12, 30), new List<GetDependentDto>()),
            new(2, "Ja", "Morant", 92365.22m, new DateOnly(1999, 8, 10), new List<GetDependentDto>
            {
                new(1, "Spouse", "Morant", new DateOnly(1998, 3, 3), Relationship.Spouse, 2),
                new(2, "Child1", "Morant", new DateOnly(2020, 6, 23), Relationship.Child, 2),
                new(3, "Child2", "Morant", new DateOnly(2021, 5, 18), Relationship.Child, 2)
            }),
            new(3, "Michael", "Jordan", 143211.12m, new DateOnly(1963, 2, 17), new List<GetDependentDto>
            {
                new(4, "DP", "Jordan", new DateOnly(1974, 1, 2), Relationship.DomesticPartner, 3)
            })
        };

        // Construct expected paginated result
        var expectedPagedResult = new PagedResult<GetEmployeeDto>(expectedEmployees, 3, 1, 10);

        await response.ShouldReturn(HttpStatusCode.OK, expectedPagedResult);
    }

    [Fact]
    [Trait("TestType", "GetSingle")]
    [Trait("Priority", "High")]
    [Trait("DataComplexity", "Simple")]
    public async Task WhenAskedForAnEmployee_ShouldReturnCorrectEmployee_UsingWebFactory()
    {
        var response = await HttpClient.GetAsync("/api/v1/employees/1");

        var expectedEmployee = new GetEmployeeDto(1, "LeBron", "James", 75420.99m, new DateOnly(1984, 12, 30), new List<GetDependentDto>());

        await response.ShouldReturn(HttpStatusCode.OK, expectedEmployee);
    }

    [Fact]
    [Trait("TestType", "GetSingle")]
    [Trait("Priority", "Medium")]
    [Trait("DataComplexity", "Simple")]
    public async Task WhenAskedForANonexistentEmployee_ShouldReturn404_UsingWebFactory()
    {
        var response = await HttpClient.GetAsync($"/api/v1/employees/999999");

        await response.ShouldReturn(HttpStatusCode.NotFound);
    }
}
